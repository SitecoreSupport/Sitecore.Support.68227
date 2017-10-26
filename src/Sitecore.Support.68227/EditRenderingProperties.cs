using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Shell.Applications.Layouts.DeviceEditor;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Specialized;

namespace Sitecore.Shell.Applications.WebEdit.Commands
{
  /// <summary>Implements the Edit command.</summary>
  [System.Serializable]
  public class EditRenderingProperties : Command
  {
    // Methods
    public override void Execute(CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      string formValue = WebUtil.GetFormValue("scLayout");
      string id = ShortID.Decode(WebUtil.GetFormValue("scDeviceID"));
      string uniqueId = ShortID.Decode(context.Parameters["uniqueid"]);
      string key = "PageDesigner";
      string str5 = Sitecore.Web.WebEditUtil.ConvertJSONLayoutToXML(formValue);
      Assert.IsNotNull(str5, "xml");
      WebUtil.SetSessionValue(key, str5);
      int index = LayoutDefinition.Parse(str5).GetDevice(id).GetIndex(uniqueId);
      NameValueCollection parameters = new NameValueCollection
      {
        ["device"] = id,
        ["handle"] = key,
        ["selectedindex"] = index.ToString()
      };
      ClientPipelineArgs args = new ClientPipelineArgs(parameters);
      Context.ClientPage.Start(this, "Run", args);
    }

    private static string GetLayout(string layout)
    {
      Assert.ArgumentNotNull(layout, "layout");
      return Sitecore.Web.WebEditUtil.ConvertXMLLayoutToJSON(layout);
    }

    protected void Run(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      int @int = MainUtil.GetInt(args.Parameters["selectedindex"], -1);
      if (@int >= 0)
      {
        Item clientContentItem = Sitecore.Web.WebEditUtil.GetClientContentItem(Client.ContentDatabase);
        RenderingParameters parameters = new RenderingParameters
        {
          Args = args,
          DeviceId = args.Parameters["device"],
          SelectedIndex = @int,
          HandleName = args.Parameters["handle"],
          Item = clientContentItem
        };
        if (parameters.Show())
        {
          if (args.HasResult)
          {
            string layout = GetLayout(WebUtil.GetSessionString(args.Parameters["handle"]));
            SheerResponse.SetAttribute("scLayoutDefinition", "value", layout);
            SheerResponse.Eval("window.parent.Sitecore.PageModes.ChromeManager.handleMessage('chrome:rendering:propertiescompleted');");
          }
          else
          {
            SheerResponse.SetAttribute("scLayoutDefinition", "value", string.Empty);
          }
          WebUtil.RemoveSessionValue(args.Parameters["handle"]);
        }
      }
    }

  }
}
