using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace KamogeloLibrary.Models
{
    public class MaintenanceViewModel
    {
        public List<authors> Authors { get; set; }

        public List<types> Types { get; set; }

        public List<borrows> Borrows { get; set; }

        public IPagedList<authors> Authors2 { get; set; }
        public IPagedList<types> Types2 { get; set; }
        public IPagedList<borrows> Borrows2 { get; set; }
    }
}