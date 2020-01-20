using System.Collections.Generic;

namespace FoodiePacks.Models
{
    public class EmailViewModel
    {
        public EmailViewModel()
        {
            PickingItems = new List<PickingItem>();

            CheeseSWs = new List<LineItem>();
            EggMayos = new List<LineItem>();
            Vegemites = new List<LineItem>();
            CheeseToms = new List<LineItem>();

            PlainPastas = new List<LineItem>();
            CheesePastas = new List<LineItem>();

            TunaAvos = new List<LineItem>();
            TunaCukes = new List<LineItem>();
            Cukes = new List<LineItem>();
            Avos = new List<LineItem>();
            Tunas = new List<LineItem>();
        }

        public List<PickingItem> PickingItems { get; set; }

        //sandwiches
        public List<LineItem> CheeseSWs { get; set; }
        public List<LineItem> CheeseToms { get; set; }
        public List<LineItem> EggMayos { get; set; }
        public List<LineItem> Vegemites { get; set; }
        public List<LineItem> TunaSWs { get; set; }


        //Pastas
        public List<LineItem> PlainPastas { get; set; }
        public List<LineItem> CheesePastas { get; set; }

        //Sushi
        public List<LineItem> TunaAvos { get; set; }
        public List<LineItem> TunaCukes { get; set; }
        public List<LineItem> Cukes { get; set; }
        public List<LineItem> Avos { get; set; }
        public List<LineItem> Tunas { get; set; }
    }
}