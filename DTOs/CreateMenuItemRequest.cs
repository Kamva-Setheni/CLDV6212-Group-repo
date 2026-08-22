using System;
using System.Collections.Generic;
using System.Text;

 
    public class CreateMenuItemRequest
    {
        //catergory of the menu item(used as Partition key)
        public string Catergory { get; set; } = string.Empty;

        //Unique stock keeping unit(used as rowkey)
        public string SKU { get; set; } = string.Empty;

        //Name of the menu item
        public string Name { get; set; } = string.Empty;

        //Name of the menu item
        public string Description { get; set; } = string.Empty;

        //selling price
        public double Price { get; set; }


        //indicates if item is available
        public bool IsAvailable { get; set; }


    }

