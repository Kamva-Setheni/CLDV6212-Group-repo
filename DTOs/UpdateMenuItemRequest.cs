using System;
using System.Collections.Generic;
using System.Text;
 
    public class UpdateMenuItemRequest
    {
        //Upadted Menu item name name
        public string Name { get; set; } = string.Empty;

        //Upadted Description
        public string Description { get; set; } = string.Empty;

        //Upadted Price
        public double Price { get; set; }

        //Upadted availability
        public bool IsAvailable { get; set; }



    }
