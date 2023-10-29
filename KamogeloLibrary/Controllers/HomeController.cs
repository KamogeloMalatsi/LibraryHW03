using KamogeloLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Net;
using PagedList;
using System.Web.UI;
using System.Drawing.Printing;
using Newtonsoft.Json;
using System.IO;
//using System.Xml.Linq;
using GemBox.Document;
using System.Windows.Controls.Primitives;

namespace KamogeloLibrary.Controllers
{
    public class HomeController : Controller
    {
        private LibraryEntities db = new LibraryEntities();
        public ActionResult Index(int?page1, int?page2)
        {
            int pageSize = 10;
            int pageNumber1 = (page1??1);
            int pageNumber2 = (page2??1);

            var students2 = db.students.ToList();
            var books2 = db.books.ToList();

            var count = db.students.Count();
            decimal totalPages = count / (decimal)pageSize;
            ViewBag.TotalPages = Math.Ceiling(totalPages);

            var count2 = db.books.Count();
            decimal totalPages2 = count2 / (decimal)pageSize;
            ViewBag.TotalPages2 = Math.Ceiling(totalPages2);

            var newViewModel = new HomeViewModel
            {
                Students2 = students2.ToPagedList(pageNumber1,pageSize),
                Books2 = books2.ToPagedList(pageNumber2,pageSize),
                Borrows = db.borrows.ToList(),
            };
            ViewBag.PageNumber1 = pageNumber1;
            ViewBag.PageNumber2 = pageNumber2;

            return View(newViewModel);
        }

        public ActionResult Maintenance(int?page1, int?page2, int?page3)
        {
            int pageSize = 5;
            int pageNumber1 = (page1 ?? 1);
            int pageNumber2 = (page2 ?? 1);
            int pageNumber3 = (page3 ?? 1);

            var authors2 = db.authors.ToList();
            var types2 = db.types.ToList();
            var borrows2 = db.borrows.ToList();

            var count = db.authors.Count();
            decimal totalPages = count / (decimal)pageSize;
            ViewBag.TotalPages = Math.Ceiling(totalPages);

            var count2 = db.types.Count();
            decimal totalPages2 = count2 / (decimal)pageSize;
            ViewBag.TotalPages2 = Math.Ceiling(totalPages2);

            var count3 = db.borrows.Count();
            decimal totalPages3 = count3 / (decimal)pageSize;
            ViewBag.TotalPages3 = Math.Ceiling(totalPages3);

            var newMViewModel = new MaintenanceViewModel
            {
                Authors2 = authors2.ToPagedList(pageNumber1, pageSize),
                Types2 = types2.ToPagedList(pageNumber2, pageSize),
                Borrows2 = borrows2.ToPagedList(pageNumber3, pageSize),
            };

            ViewBag.PageNumber1 = pageNumber1;
            ViewBag.PageNumber2 = pageNumber2;
            ViewBag.PageNumber3 = pageNumber3;

            return View(newMViewModel);
        }

        public ActionResult Report()
        {
            var popularityResults = db.books
                .Join(db.borrows, book => book.bookId, borrow => borrow.bookId,
                (book, borrow) => new { Book = book, Borrow = borrow })
                .GroupBy(x => x.Book.name)
                .Select(group => new
                {
                    Name = group.Key,
                    Popularity = group.Count()
                })
                .OrderByDescending(x => x.Popularity)
                .Take(10)
                .ToList();

            List<DataPoint> dataPoints = new List<DataPoint>();

            foreach (var item in popularityResults)
            {
                dataPoints.Add(new DataPoint(item.Name,item.Popularity));                   
            }

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);

            return View();
        }

        public ActionResult GetDataByYear(int selectedYear)
        {
            var popularityResults = (from book in db.books
                                     join borrow in db.borrows on book.bookId equals borrow.bookId
                                     where borrow.takenDate.Value.Year == selectedYear
                                     group book by book.name into bookGroup
                                     select new
                                     {
                                         Name = bookGroup.Key,
                                         Popularity = bookGroup.Count()
                                     })
                                     .OrderByDescending(x => x.Popularity)
                                     .Take(10)
                                     .ToList();

            List<DataPoint> dataPoints = new List<DataPoint>();

            foreach (var item in popularityResults)
            {
                dataPoints.Add(new DataPoint(item.Name, item.Popularity));
            }

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
            var Results = JsonConvert.SerializeObject(dataPoints);
            //return Results;
            return Content(Results, "application/json");
            //return View();
        }

        //public ActionResult TinyMCE() => this.View(new FileModel());

        public ActionResult Reporting()
        {
            var popularityResults = db.books
                .Join(db.borrows, book => book.bookId, borrow => borrow.bookId,
                (book, borrow) => new { Book = book, Borrow = borrow })
                .GroupBy(x => x.Book.name)
                .Select(group => new
                {
                    Name = group.Key,
                    Popularity = group.Count()
                })
                .OrderByDescending(x => x.Popularity)
                .Take(10)
                .ToList();

            List<DataPoint> dataPoints = new List<DataPoint>();

            foreach (var item in popularityResults)
            {
                dataPoints.Add(new DataPoint(item.Name, item.Popularity));
            }

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);


            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Documents/"));
            List<FileModel> files = new List<FileModel>();
            foreach (string filePath in filePaths)
            {
                files.Add(new FileModel { FileName = Path.GetFileName(filePath) });
            }

            return View(files);

            //return View(new FileModel());
        }

        [HttpPost]
        public FileResult SAVE(string content, string extension, string fileName)
        {
            // If using Professional version, put your serial key below.
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");

            var templateFile = Server.MapPath("~/App_Data/DocumentTemplate.docx");

            // Load template document.
            var document = DocumentModel.Load(templateFile);

            // Insert content from HTML editor.
            var bookmark = document.Bookmarks["HtmlBookmark"];
            bookmark.GetContent(true).LoadText(content, LoadOptions.HtmlDefault);


            // Save document to stream in specified format.
            var saveOptions = GetSaveOptions(extension);
            var stream = new MemoryStream();
            document.Save(stream, saveOptions);

            // Download document.
            var downloadDirectory = Server.MapPath("~/Documents/");
            var downloadFile = $"{fileName}{extension}";
            var fullPath = Path.Combine(downloadDirectory, downloadFile);
            System.IO.File.WriteAllBytes(fullPath, stream.ToArray());

            string[] filePaths = Directory.GetFiles(Server.MapPath("~/Documents/"));
            List<FileModel> files = new List<FileModel>();
            foreach (string filePath in filePaths)
            {
                files.Add(new FileModel { FileName = Path.GetFileName(filePath) });
            }
            return File(fullPath, saveOptions.ContentType, downloadFile);
        }

        private static SaveOptions GetSaveOptions(string extension)
        {
            switch (extension)
            {
                case ".docx": return SaveOptions.DocxDefault;
                case ".pdf": return SaveOptions.PdfDefault;
                case ".xps": return SaveOptions.XpsDefault;
                case ".html": return SaveOptions.HtmlDefault;
                case ".mhtml": return new HtmlSaveOptions() { HtmlType = HtmlType.Mhtml };
                case ".rtf": return SaveOptions.RtfDefault;
                case ".xml": return SaveOptions.XmlDefault;
                case ".png": return SaveOptions.ImageDefault;
                case ".jpeg": return new ImageSaveOptions(ImageSaveFormat.Jpeg);
                case ".gif": return new ImageSaveOptions(ImageSaveFormat.Gif);
                case ".bmp": return new ImageSaveOptions(ImageSaveFormat.Bmp);
                case ".tiff": return new ImageSaveOptions(ImageSaveFormat.Tiff);
                case ".wmp": return new ImageSaveOptions(ImageSaveFormat.Wmp);
                default: return SaveOptions.TxtDefault;
            }
        }

        public FileResult DownloadFile(string fileName)
        {
            string path = Server.MapPath("~/Documents/") + fileName;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, "application/octet-stream", fileName);
        }

        public ActionResult DeleteFile(string fileName)
        {
            string path = Server.MapPath("~/Documents/") + fileName;
            byte[] bytes = System.IO.File.ReadAllBytes(path);


            System.IO.File.Delete(path);

            return RedirectToAction("Reporting");
        }

        public FileResult DisplayFile(string fileName)
        {
            string path = Server.MapPath("~/Documents/") + fileName;
            byte[] bytes = System.IO.File.ReadAllBytes(path);
            return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }


        //Redirect To Students
        public ActionResult CreateStudent()
        {
            return RedirectToAction("Create", "students");
        }

        //Redirect To Books
        public ActionResult CreateBook()
        {
            return RedirectToAction("Create", "books");
        }

        //Redirect to Authors
        public ActionResult CreateAuthor()
        {
            return RedirectToAction("Create", "authors");
        }
        public ActionResult EditAuthor(int id)
        {
            return RedirectToAction("Edit", "authors", new { id = id });
        }

        public ActionResult DeleteAuthor(int id)
        {
            return RedirectToAction("Delete", "authors", new { id = id });
        }

        public ActionResult DetailsAuthor(int id)
        {
            return RedirectToAction("Details", "authors", new { id = id });
        }

        //Redirect to Types
        public ActionResult CreateType()
        {
            return RedirectToAction("Create", "types");
        }
        public ActionResult EditType(int id)
        {
            return RedirectToAction("Edit", "types", new { id = id });
        }

        public ActionResult DeleteType(int id)
        {
            return RedirectToAction("Delete", "types", new { id = id });
        }

        public ActionResult DetailsType(int id)
        {
            return RedirectToAction("Details", "types", new { id = id });
        }

        //Redirect to Borrows
        public ActionResult CreateBorrow()
        {
            return RedirectToAction("Create", "borrows");
        }
        public ActionResult EditBorrow(int id)
        {
            return RedirectToAction("Edit", "borrows", new { id = id });
        }

        public ActionResult DeleteBorrow(int id)
        {
            return RedirectToAction("Delete", "borrows", new { id = id });
        }

        public ActionResult DetailsBorrow(int id)
        {
            return RedirectToAction("Details", "borrows", new { id = id });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}