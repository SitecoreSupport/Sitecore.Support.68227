using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Shell.Applications.Layouts.DeviceEditor;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Sitecore.Support.Shell.Applications.WebEdit.Commands
{
  /// <summary>Implements the Edit command.</summary>
  [System.Serializable]
  public class EditRenderingProperties : Command
  {
    // Fields
    private static List<OpenedItems> openedItems = new List<OpenedItems>();

    // Methods
    public override void Execute(CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      string formValue = WebUtil.GetFormValue("scLayout");
      string id = ShortID.Decode(WebUtil.GetFormValue("scDeviceID"));
      string uniqueId = ShortID.Decode(context.Parameters["uniqueid"]);
      string str4 = Sitecore.Web.WebEditUtil.ConvertJSONLayoutToXML(formValue);
      Assert.IsNotNull(str4, "xml");
      OpenedItems myclass = new OpenedItems();
      string key = "PageDesigner" + openedItems.Count.ToString();
      WebUtil.SetSessionValue(key, str4);
      myclass.Pagedesigner = key;
      int index = LayoutDefinition.Parse(str4).GetDevice(id).GetIndex(uniqueId);
      NameValueCollection parameters = new NameValueCollection
      {
        ["device"] = id,
        ["selectedindex"] = index.ToString()
      };
      myclass.ItemId = context.Items.First<Item>().ID.ToString();
      string str6 = "handle" + openedItems.Count.ToString();
      myclass.Handle = str6;
      if (openedItems.Count > 0)
      {
        int num3 = openedItems.FindIndex(x => x.ItemId.Equals(myclass.ItemId));
        if (num3 >= 0)
        {
          openedItems[num3] = myclass;
        }
        else
        {
          openedItems.Add(myclass);
        }
      }
      else
      {
        openedItems.Add(myclass);
      }
      parameters[str6] = key;
      ClientPipelineArgs args = new ClientPipelineArgs(parameters);
      Context.ClientPage.Start(this, "Run", args);
    }

    private static string GetLayout(string layout)
    {
      Assert.ArgumentNotNull(layout, "layout");
      return Sitecore.Web.WebEditUtil.ConvertXMLLayoutToJSON(layout);
    }

    private void RemoveOpenedItemFromSession(OpenedItems singleobject)
    {
      openedItems.Remove(singleobject);
    }

    protected void Run(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      int @int = MainUtil.GetInt(args.Parameters["selectedindex"], -1);
      if (@int >= 0)
      {
        Item clientContentItem = Sitecore.Web.WebEditUtil.GetClientContentItem(Client.ContentDatabase);
        OpenedItems item = openedItems.Single<OpenedItems>(r => r.ItemId.Equals(clientContentItem.ID.ToString()));
        RenderingParameters parameters = new RenderingParameters
        {
          Args = args,
          DeviceId = args.Parameters["device"],
          SelectedIndex = @int,
          HandleName = args.Parameters[item.Handle],
          Item = clientContentItem
        };
        if (parameters.Show())
        {
          if (args.HasResult)
          {
            string sessionString = WebUtil.GetSessionString(args.Parameters[item.Handle]);
            if (!string.IsNullOrEmpty(sessionString))
            {
              string layout = GetLayout(sessionString);
              SheerResponse.SetAttribute("scLayoutDefinition", "value", layout);
              SheerResponse.Eval("window.parent.Sitecore.PageModes.ChromeManager.handleMessage('chrome:rendering:propertiescompleted');");
            }
            else
            {
              Log.Warn("We were protected from issue because of simultaneous editing", this);
              SheerResponse.SetAttribute("scLayoutDefinition", "value", string.Empty);
            }
          }
          else
          {
            SheerResponse.SetAttribute("scLayoutDefinition", "value", string.Empty);
          }
          WebUtil.RemoveSessionValue(args.Parameters[item.Handle]);
          openedItems.Remove(item);
        }
      }
    }

  }
}
