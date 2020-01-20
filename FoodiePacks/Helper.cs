using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace FoodiePacks
{
    public static class Helper
    {
        public static HtmlString MenuLinkSide(this HtmlHelper helper, string text, string action, string controller, string menuGroup)
        {
            var routeData = helper.ViewContext.RouteData.Values;
            var currentController = routeData["controller"];
            var currentAction = routeData["action"];
            var currentMenu = routeData["Id"];
            var visibility = String.Equals(menuGroup, currentMenu as string, StringComparison.OrdinalIgnoreCase) ? "visible" : "hidden";

            var builder = new StringBuilder();
            builder.Append(string.Format("<li data-menugroup=\"{0}\" class=\"{1}\">", menuGroup, visibility));
            if (String.Equals(action, currentAction as string, StringComparison.OrdinalIgnoreCase) && String.Equals(controller, currentController as string, StringComparison.OrdinalIgnoreCase))
            {
                builder.Append(helper.ActionLink(text, action, controller, new { menuGroup }, new { @class = "currentMenuItemSide" }));
                builder.Append("</li>");
                return new HtmlString(builder.ToString());
            }
            else
            {
                builder.Append(helper.ActionLink(text, action, controller, new { menuGroup }, null));
                builder.Append("</li>");
                return new HtmlString(builder.ToString());
            }
        }
    }
}