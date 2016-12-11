namespace AutoList.Control.Enums
{
   using System;
   using System.Collections.Generic;
   using System.Linq;
   using System.Text;

   public enum AutoListSelectionMode
   {
      /// <summary>
      /// Items will not be selectable
      /// </summary>
      None = 1,
      /// <summary>
      /// Items are selectable by radio button
      /// </summary>
      Single = 2,
      /// <summary>
      /// Items are selectable by checkbox
      /// </summary>
      Multiple = 3
   }
}
