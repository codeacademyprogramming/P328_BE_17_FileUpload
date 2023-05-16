using Microsoft.AspNetCore.Mvc;
using P328Pustok.DAL;
using P328Pustok.Helpers;
using P328Pustok.Models;
using P328Pustok.ViewModels;

namespace P328Pustok.Areas.Manage.Controllers
{
    [Area("manage")]
    public class SliderController : Controller
    {
        private readonly PustokContext _context;
        private readonly IWebHostEnvironment _env;

        public SliderController(PustokContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public IActionResult Index(int page=1)
        {
            var query = _context.Sliders.OrderBy(x=>x.Order).AsQueryable();

            return View(PaginatedList<Slider>.Create(query,page,2));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Slider slider)
        {
            if (!ModelState.IsValid) return View();

            if (slider.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "ImageFile is required");
                return View();
            }

            if(slider.ImageFile.ContentType != "image/jpeg" && slider.ImageFile.ContentType != "image/png")
            {
                ModelState.AddModelError("ImageFile", "Content type must be jpeg,jpg or png");
                return View();
            }

            if(slider.ImageFile.Length>2097152)
            {
                ModelState.AddModelError("ImageFile", "File size must be less than 2MB");
                return View();
            }

            foreach (var item in _context.Sliders.Where(x=>x.Order>=slider.Order))
            {
                item.Order++;
            }

            slider.ImageName = FileManager.Save(_env.WebRootPath, "uploads/sliders", slider.ImageFile);

            _context.Sliders.Add(slider);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            Slider slider = _context.Sliders.Find(id);

            if(slider == null) return View("Error");

            return View(slider);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Slider slider)
        {
            if(!ModelState.IsValid) return View();

            string fileName=null;
            if (slider.ImageFile != null)
            {
                if (slider.ImageFile.ContentType != "image/jpeg" && slider.ImageFile.ContentType != "image/png")
                {
                    ModelState.AddModelError("ImageFile", "Content type must be jpeg,jpg or png");
                    return View();
                }

                if (slider.ImageFile.Length > 2097152)
                {
                    ModelState.AddModelError("ImageFile", "File size must be less than 2MB");
                    return View();
                }


                fileName = FileManager.Save(_env.WebRootPath, "uploads/sliders", slider.ImageFile);
            }

            Slider existSlider = _context.Sliders.Find(slider.Id);

            if (existSlider == null) return View("Error");

            existSlider.Order = slider.Order;
            existSlider.Title1 = slider.Title1;
            existSlider.Title2 = slider.Title2;
            existSlider.BtnUrl = slider.BtnUrl; 
            existSlider.BtnText= slider.BtnText;
            existSlider.Desc = slider.Desc;
            existSlider.ImageName = fileName??existSlider.ImageName;


            _context.SaveChanges();
            return RedirectToAction("Index");
        }



    }
}
