using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FoodiePacks.Controllers;
using FoodiePacks.Models;
using Newtonsoft.Json;

namespace FoodiePacks
{
    public class OrderManager
    {
        public OrderManager()
        {
            Trios = new List<KeyValuePair<string, string>>();
        }

        public List<KeyValuePair<string, string>> Trios { get; set; }

        public HtmlString Download()
        {
            var builder = new StringBuilder();
            using (var client = new WebClient())
            {
                string credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes("363831486367c73e0735558753106d51" + ":" + "5e4c39cf4a2fe6c06dcb11f902125f81"));
                client.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", credentials);
                var ordersJson = client.DownloadString("https://@foodie-packs.myshopify.com/admin/orders.json");
                var ordersRoot = JsonConvert.DeserializeObject<OrdersRoot>(ordersJson);

                var subscriptionOrders = GetSubscriptionOrdersInWeek(ordersRoot);

                var ordersInWeek = GetOrdersInWeek(ordersRoot, subscriptionOrders);


                Trios.Add(new KeyValuePair<string, string>("Sandwich", "Trio 1 & 2"));
                Trios.Add(new KeyValuePair<string, string>("Pasta", "Trio 3 & 4"));
                Trios.Add(new KeyValuePair<string, string>("Sushi", "Trio 5 & 6"));

                builder.Append("<h2>Date: " + DateTime.UtcNow.AddHours(11).Date + "</h2>");

                builder.Append("<h3>Pack Type Qty</h3>");
                builder.Append(CalculatePackTypeQuantities(ordersInWeek, Trios));

                builder.Append("<h3>User Orders</h3>");
                builder.Append(CalculateUserOrders(ordersInWeek));

                //builder.Append(CalculateSubscriptionOrder)

                builder.Append("<h3>Food Quantities</h3>");
                builder.Append(CalculateFoodAmounts(ordersInWeek));
            }

            return new HtmlString(builder.ToString());
        }

        public HtmlString CalculatePackTypeQuantities(List<Order> orders, List<KeyValuePair<string, string>> trios)
        {
            var builder = new StringBuilder();
            var lineItems = new List<LineItem>();

            foreach (var order in orders)
            {
                lineItems.AddRange(order.line_items);
            }

            var sandwichTrioCount = 0;
            var pastaTrioCount = 0;
            var sushiTrioCount = 0;

            builder.Append("<ul>");
            foreach (var item in lineItems.GroupBy(l => l.name).OrderBy(l => l.Key))
            {
                var trio = "";
                var packTypeName = item.First().name;
                if (packTypeName.Contains("Sandwich"))
                {
                    trio = trios.First(t => t.Key == "Sandwich").Value;
                    sandwichTrioCount += item.Sum(x => x.quantity ?? 0);
                    builder.Append("<li>" + item.Sum(x => x.quantity ?? 0) + " " + item.First().name + " - " + item.Sum(x => x.quantity ?? 0) + " x " + trio + " </li>");
                }
                else if
                (packTypeName.Contains("Pasta"))
                {
                    trio = trios.First(t => t.Key == "Pasta").Value;
                    pastaTrioCount += item.Sum(x => x.quantity ?? 0);
                    builder.Append("<li>" + item.Sum(x => x.quantity ?? 0) + " " + item.First().name + " - " + item.Sum(x => x.quantity ?? 0) + " x " + trio + " </li>");
                }
                else if
                (packTypeName.Contains("Sushi"))
                {
                    trio = trios.First(t => t.Key == "Sushi").Value;
                    sushiTrioCount += item.Sum(x => x.quantity ?? 0);
                    builder.Append("<li>" + item.Sum(x => x.quantity ?? 0) + " " + item.First().name + " - " + item.Sum(x => x.quantity ?? 0) + " x " + trio + " </li>");
                }
                else if
                (packTypeName.Contains("Box"))
                {
                    builder.Append("<li>" + item.Sum(x => x.quantity ?? 0) + " " + item.First().name + "</li>");
                }
            }

            builder.Append("</ul>");
            builder.Append("<hr/>");
            builder.Append("<label>" + sandwichTrioCount + " - Trio 1 & 2</label><br/>");
            builder.Append("<label>" + pastaTrioCount + " - Trio 3 & 4</label><br/>");
            builder.Append("<label>" + sushiTrioCount + " - Trio 5 & 6</label><br/>");

            return new HtmlString(builder.ToString());
        }

        public string CalculateFoodAmounts(List<Order> orders)
        {
            var controller = CreateController<HomeController>();
            var viewModel = new EmailViewModel();

            var lineItems = new List<LineItem>();
            foreach (var order in orders)
            {
                lineItems.AddRange(order.line_items);
            }

            viewModel.PickingItems = JsonConvert.DeserializeObject<List<PickingItem>>(File.ReadAllText(HttpContext.Current.Server.MapPath("~/pickingItems.json")));

            //viewModel.Vegemites.AddRange(lineItems.Where(l => l.name == "Sandwich Lunch Packs - Vegemite").ToList());
            viewModel.Vegemites.AddRange(lineItems.Where(l => l.name.Contains("Sandwich Lunch Packs") && l.name.Contains("Vegemite")).ToList());

            //viewModel.CheeseSWs.AddRange(lineItems.Where(l => l.name == "Sandwich Lunch Packs - Cheese").ToList());
            viewModel.CheeseSWs.AddRange(lineItems.Where(l => l.name.Contains("Sandwich Lunch Packs") && l.name.Contains("Cheese") && !l.name.Contains("Tomato")).ToList());

            //viewModel.CheeseToms.AddRange(lineItems.Where(l => l.name == "Sandwich Lunch Packs - Cheese Tomato").ToList());
            viewModel.CheeseToms.AddRange(lineItems.Where(l => l.name.Contains("Sandwich Lunch Packs") && l.name.Contains("Cheese Tomato")).ToList());

            //viewModel.EggMayos.AddRange(lineItems.Where(l => l.name == "Sandwich Lunch Packs - Egg Mayo (dairy free)").ToList());
            viewModel.EggMayos.AddRange(lineItems.Where(l => l.name.Contains("Sandwich Lunch Packs") && l.name.Contains("Egg Mayo (dairy free)")).ToList());

            //viewModel.EggMayos.AddRange(lineItems.Where(l => l.name == "Sandwich Lunch Packs - Tuna").ToList());
            viewModel.EggMayos.AddRange(lineItems.Where(l => l.name.Contains("Sandwich Lunch Packs") && l.name.Contains("Tuna")).ToList());



            //viewModel.PlainPastas.AddRange(lineItems.Where(l => l.name == "Pasta Lunch Packs - Classic Plain Pasta").ToList());
            viewModel.PlainPastas.AddRange(lineItems.Where(l => l.name.Contains("Pasta Lunch Packs") && l.name.Contains("Classic Plain Pasta")).ToList());

            //viewModel.CheesePastas.AddRange(lineItems.Where(l => l.name == "Pasta Lunch Packs - Classic Plain Pasta with Grated Cheese").ToList());
            viewModel.CheesePastas.AddRange(lineItems.Where(l => l.name.Contains("Pasta Lunch Packs") && l.name.Contains("Classic Plain Pasta with Grated Cheese")).ToList());



            //viewModel.TunaAvos.AddRange(lineItems.Where(l => l.name == "Sushi Lunch Packs - Tuna Avocado").ToList());
            viewModel.TunaAvos.AddRange(lineItems.Where(l => l.name.Contains("Sushi Lunch Packs") && l.name.Contains("Tuna Avocado")).ToList());

            //viewModel.TunaCukes.AddRange(lineItems.Where(l => l.name == "Sushi Lunch Packs - Tuna Cucumber").ToList());
            viewModel.TunaCukes.AddRange(lineItems.Where(l => l.name.Contains("Sushi Lunch Packs") && l.name.Contains("Tuna Cucumber")).ToList());

            //viewModel.Cukes.AddRange(lineItems.Where(l => l.name == "Sushi Lunch Packs - Cucumber").ToList());
            viewModel.Cukes.AddRange(lineItems.Where(l => l.name.Contains("Sushi Lunch Packs") && l.name.Contains("Cucumber") && !l.name.Contains("Tuna")).ToList());

            //viewModel.Avos.AddRange(lineItems.Where(l => l.name == "Sushi Lunch Packs - Avocado").ToList());
            viewModel.Avos.AddRange(lineItems.Where(l => l.name.Contains("Sushi Lunch Packs") && l.name.Contains("Avocado") && !l.name.Contains("Tuna")).ToList());

            //viewModel.Tunas.AddRange(lineItems.Where(l => l.name == "Sushi Lunch Packs - Tuna").ToList());
            viewModel.Tunas.AddRange(lineItems.Where(l => l.name.Contains("Sushi Lunch Packs") && l.name.Contains("Tuna") && !l.name.Contains("Cucumber") && !l.name.Contains("Avocado")).ToList());

            return ViewRenderer.RenderPartialView("~/Views/Home/FoodAmounts.cshtml", viewModel, controller.ControllerContext);
        }

        public List<Order> GetOrdersInWeek(OrdersRoot obj, List<Order> subscriptionOrders)
        {
            var weekAgoUtc = DateTimeOffset.UtcNow.AddDays(-7);
            var todayUtc = DateTimeOffset.UtcNow;
            var orders = obj.orders.Where(o => DateTimeOffset.Parse(o.created_at).UtcDateTime > weekAgoUtc && DateTimeOffset.Parse(o.created_at).UtcDateTime < todayUtc).ToList();

            //filter out the subscription orders
            orders = orders.Where(o => !subscriptionOrders.Any(so => so.id == o.id)).ToList();

            //add back the subscription orders with only the weeks line items
            orders.AddRange(subscriptionOrders);

            return orders;
        }

        public List<Order> GetSubscriptionOrdersInWeek(OrdersRoot obj)
        {
            var subscriptionOrders = obj.orders.Where(o => o.line_items.Any(l => l.properties.Any())).ToList();

            var ordersWithLineItemsForWeek = new List<Order>();

            foreach (var so in subscriptionOrders)
            {
                var x = so.line_items.SelectMany(l => l.properties).Cast<dynamic>();
                var y = x.First(pe => pe["name"] == "start-date");

                string dateObj = (string)y["value"];

                DateTime fromDate = DateTime.Parse(dateObj, new CultureInfo("en-US"));

                int difference = Convert.ToInt32((DateTime.Now.Date - fromDate.Date).TotalDays);

                int noOfWeeks = difference/7;

                string week = ((noOfWeeks % 4) + 1).ToString();

                List<LineItem> lineItemsInWeek = so.line_items.
                    Where(li => li.properties.Cast<dynamic>().Any(lp => lp["name"] == "week" && lp["value"] == week)).ToList();

                so.line_items = lineItemsInWeek;

                ordersWithLineItemsForWeek.Add(so);
            }

            return ordersWithLineItemsForWeek;
        }

        public string CalculateUserOrders(List<Order> orders)
        {
            var controller = CreateController<HomeController>();

            return ViewRenderer.RenderPartialView("~/Views/Home/UserOrder.cshtml", orders, controller.ControllerContext);
        }

        public void SendWeeklyOrderEmail()
        {
            SendEmail(Download().ToString());
        }

        public void SendSubscriptionOrder(List<SubscriptionProduct> products)
        {
            var controller = CreateController<HomeController>();

            var emailBody = ViewRenderer.RenderPartialView("~/Views/Home/SubscriptionOrder.cshtml", products, controller.ControllerContext);
            SendEmail(emailBody);
        }

        public void SendEmail(string emailBody)
        {
            var fromEmail = ConfigurationManager.AppSettings["FoodiePacksEmail"];
            var password = ConfigurationManager.AppSettings["Password"];
            var siteOwnerEmail = ConfigurationManager.AppSettings["OwnerEmail"];

            var email = new MailMessage();
            email.From = new MailAddress(fromEmail, "Weekly pack orders");
            email.To.Add(new MailAddress("basti.fjastad@gmail.com", "Weekly pack orders"));
            email.To.Add(new MailAddress(siteOwnerEmail, "Weekly pack orders"));
            email.IsBodyHtml = true;
            email.Body = emailBody;


            SmtpClient smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
                Port = 587,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 10000
            };

            smtp.Send(email);
        }

        #region ViewRendering
        public static T CreateController<T>(RouteData routeData = null)
           where T : Controller, new()
        {
            // create a disconnected controller instance
            T controller = new T();

            // get context wrapper from HttpContext if available
            HttpContextBase wrapper = null;
            if (HttpContext.Current != null)
                wrapper = new HttpContextWrapper(System.Web.HttpContext.Current);
            else
                throw new InvalidOperationException(
                    "Can't create Controller Context if no active HttpContext instance is available.");

            if (routeData == null)
                routeData = new RouteData();

            // add the controller routing if not existing
            if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
                routeData.Values.Add("controller", controller.GetType().Name
                                                            .ToLower()
                                                            .Replace("controller", ""));

            controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
            return controller;
        }

        /// <summary>
        /// Class that renders MVC views to a string using the
        /// standard MVC View Engine to render the view. 
        /// </summary>
        public class ViewRenderer
        {
            /// <summary>
            /// Required Controller Context
            /// </summary>
            protected ControllerContext Context { get; set; }

            /// <summary>
            /// Initializes the ViewRenderer with a Context.
            /// </summary>
            /// <param name="controllerContext">
            /// If you are running within the context of an ASP.NET MVC request pass in
            /// the controller's context. 
            /// Only leave out the context if no context is otherwise available.
            /// </param>
            public ViewRenderer(ControllerContext controllerContext = null)
            {
                // Create a known controller from HttpContext if no context is passed
                if (controllerContext == null)
                {
                    if (HttpContext.Current != null)
                        controllerContext = CreateController<HomeController>().ControllerContext;
                    else
                        throw new InvalidOperationException(
                            "ViewRenderer must run in the context of an ASP.NET " +
                            "Application and requires HttpContext.Current to be present.");
                }
                Context = controllerContext;
            }

            /// <summary>
            /// Renders a full MVC view to a string. Will render with the full MVC
            /// View engine including running _ViewStart and merging into _Layout        
            /// </summary>
            /// <param name="viewPath">
            /// The path to the view to render. Either in same controller, shared by 
            /// name or as fully qualified ~/ path including extension
            /// </param>
            /// <param name="model">The model to render the view with</param>
            /// <returns>String of the rendered view or null on error</returns>
            public string RenderView(string viewPath, object model)
            {
                return RenderViewToStringInternal(viewPath, model, false);
            }


            /// <summary>
            /// Renders a partial MVC view to string. Use this method to render
            /// a partial view that doesn't merge with _Layout and doesn't fire
            /// _ViewStart.
            /// </summary>
            /// <param name="viewPath">
            /// The path to the view to render. Either in same controller, shared by 
            /// name or as fully qualified ~/ path including extension
            /// </param>
            /// <param name="model">The model to pass to the viewRenderer</param>
            /// <returns>String of the rendered view or null on error</returns>
            public string RenderPartialView(string viewPath, object model)
            {
                return RenderViewToStringInternal(viewPath, model, true);
            }

            /// <summary>
            /// Renders a partial MVC view to string. Use this method to render
            /// a partial view that doesn't merge with _Layout and doesn't fire
            /// _ViewStart.
            /// </summary>
            /// <param name="viewPath">
            /// The path to the view to render. Either in same controller, shared by 
            /// name or as fully qualified ~/ path including extension
            /// </param>
            /// <param name="model">The model to pass to the viewRenderer</param>
            /// <param name="controllerContext">Active Controller context</param>
            /// <returns>String of the rendered view or null on error</returns>
            public static string RenderView(string viewPath, object model,
                                            ControllerContext controllerContext)
            {
                ViewRenderer renderer = new ViewRenderer(controllerContext);
                return renderer.RenderView(viewPath, model);
            }

            /// <summary>
            /// Renders a partial MVC view to string. Use this method to render
            /// a partial view that doesn't merge with _Layout and doesn't fire
            /// _ViewStart.
            /// </summary>
            /// <param name="viewPath">
            /// The path to the view to render. Either in same controller, shared by 
            /// name or as fully qualified ~/ path including extension
            /// </param>
            /// <param name="model">The model to pass to the viewRenderer</param>
            /// <param name="controllerContext">Active Controller context</param>
            /// <param name="errorMessage">optional out parameter that captures an error message instead of throwing</param>
            /// <returns>String of the rendered view or null on error</returns>
            public static string RenderView(string viewPath, object model,
                                            ControllerContext controllerContext,
                                            out string errorMessage)
            {
                errorMessage = null;
                try
                {
                    ViewRenderer renderer = new ViewRenderer(controllerContext);
                    return renderer.RenderView(viewPath, model);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.GetBaseException().Message;
                }
                return null;
            }

            /// <summary>
            /// Renders a partial MVC view to string. Use this method to render
            /// a partial view that doesn't merge with _Layout and doesn't fire
            /// _ViewStart.
            /// </summary>
            /// <param name="viewPath">
            /// The path to the view to render. Either in same controller, shared by 
            /// name or as fully qualified ~/ path including extension
            /// </param>
            /// <param name="model">The model to pass to the viewRenderer</param>
            /// <param name="controllerContext">Active controller context</param>
            /// <returns>String of the rendered view or null on error</returns>
            public static string RenderPartialView(string viewPath, object model,
                                                    ControllerContext controllerContext)
            {
                ViewRenderer renderer = new ViewRenderer(controllerContext);
                return renderer.RenderPartialView(viewPath, model);
            }

            /// <summary>
            /// Renders a partial MVC view to string. Use this method to render
            /// a partial view that doesn't merge with _Layout and doesn't fire
            /// _ViewStart.
            /// </summary>
            /// <param name="viewPath">
            /// The path to the view to render. Either in same controller, shared by 
            /// name or as fully qualified ~/ path including extension
            /// </param>
            /// <param name="model">The model to pass to the viewRenderer</param>
            /// <param name="controllerContext">Active controller context</param>
            /// <param name="errorMessage">optional output parameter to receive an error message on failure</param>
            /// <returns>String of the rendered view or null on error</returns>
            public static string RenderPartialView(string viewPath, object model,
                                                    ControllerContext controllerContext,
                                                    out string errorMessage)
            {
                errorMessage = null;
                try
                {
                    ViewRenderer renderer = new ViewRenderer(controllerContext);
                    return renderer.RenderPartialView(viewPath, model);
                }
                catch (Exception ex)
                {
                    errorMessage = ex.GetBaseException().Message;
                }
                return null;
            }

            /// <summary>
            /// Internal method that handles rendering of either partial or 
            /// or full views.
            /// </summary>
            /// <param name="viewPath">
            /// The path to the view to render. Either in same controller, shared by 
            /// name or as fully qualified ~/ path including extension
            /// </param>
            /// <param name="model">Model to render the view with</param>
            /// <param name="partial">Determines whether to render a full or partial view</param>
            /// <returns>String of the rendered view</returns>
            protected string RenderViewToStringInternal(string viewPath, object model,
                                                        bool partial = false)
            {
                // first find the ViewEngine for this view
                ViewEngineResult viewEngineResult = null;
                if (partial)
                    viewEngineResult = ViewEngines.Engines.FindPartialView(Context, viewPath);
                else
                    viewEngineResult = ViewEngines.Engines.FindView(Context, viewPath, null);

                if (viewEngineResult == null)
                    throw new FileNotFoundException();

                // get the view and attach the model to view data
                var view = viewEngineResult.View;
                Context.Controller.ViewData.Model = model;

                string result = null;

                using (var sw = new StringWriter())
                {
                    var ctx = new ViewContext(Context, view,
                                                Context.Controller.ViewData,
                                                Context.Controller.TempData,
                                                sw);
                    view.Render(ctx, sw);
                    result = sw.ToString();
                }

                return result;
            }


            /// <summary>
            /// Creates an instance of an MVC controller from scratch 
            /// when no existing ControllerContext is present       
            /// </summary>
            /// <typeparam name="T">Type of the controller to create</typeparam>
            /// <returns></returns>
            public static T CreateController<T>(RouteData routeData = null)
                        where T : Controller, new()
            {
                T controller = new T();

                // Create an MVC Controller Context
                HttpContextBase wrapper = null;
                if (HttpContext.Current != null)
                    wrapper = new HttpContextWrapper(System.Web.HttpContext.Current);
                //else
                //    wrapper = CreateHttpContextBase(writer);


                if (routeData == null)
                    routeData = new RouteData();

                if (!routeData.Values.ContainsKey("controller") && !routeData.Values.ContainsKey("Controller"))
                    routeData.Values.Add("controller", controller.GetType().Name
                                                                .ToLower()
                                                                .Replace("controller", ""));

                controller.ControllerContext = new ControllerContext(wrapper, routeData, controller);
                return controller;
            }
        }
        #endregion
    }
}