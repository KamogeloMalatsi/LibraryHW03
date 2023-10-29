using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace KamogeloLibrary.Models
{
    public class HomeViewModel
    {
        public List<students> Students { get; set; }

        public List<books> Books { get; set; }

        public List<borrows> Borrows { get; set; }

        public IPagedList<students> Students2 { get; set; }
        public IPagedList<books> Books2 { get; set; }
    }
}