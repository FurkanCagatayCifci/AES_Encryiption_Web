using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Security.Cryptography;
using YourNamespace.Models;

namespace WebApplication2.Controllers
{
    public class HomeController : Controller
    {
        private readonly int _keySize = 256; // Seçilen anahtar boyutuna göre değiştirilebilir
        private readonly EncryptionModel _encryptionModel;

        public HomeController()
        {
            _encryptionModel = new EncryptionModel(_keySize);
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Encrypt(IFormFile audioFile, IFormFile imageFile, IFormFile imageToEncrypt)
        {
            if (audioFile == null || imageFile == null || imageToEncrypt == null)
            {
                ViewBag.Error = "Lütfen ses, görüntü ve şifrelenecek görüntü dosyalarını seçin.";
                return View("Index");
            }

            // 1. Ses ve görüntü dosyasını okuyarak anahtar oluştur
            byte[] combinedData;
            using (var audioStream = audioFile.OpenReadStream())
            using (var imageStream = imageFile.OpenReadStream())
            using (var ms = new MemoryStream())
            {
                audioStream.CopyTo(ms);
                imageStream.CopyTo(ms);
                combinedData = ms.ToArray();
            }

            // Anahtar üret
            using (var sha256 = SHA256.Create())
            {
                var key = sha256.ComputeHash(combinedData);
                key = key.Length > _keySize / 8 ? key[..(_keySize / 8)] : key;

                // 2. Görüntü dosyasını şifrele
                byte[] imageData;
                using (var imageStream = imageToEncrypt.OpenReadStream())
                using (var ms = new MemoryStream())
                {
                    imageStream.CopyTo(ms);
                    imageData = ms.ToArray();
                }

                var encryptedData = _encryptionModel.EncryptData(imageData, key);

                // Şifrelenmiş veriyi bir temp dosyasına kaydet
                TempData["EncryptedImage"] = Convert.ToBase64String(encryptedData);
            }

            return View("Index");
        }

        [HttpPost]
        public IActionResult Decrypt()
        {
            if (TempData["EncryptedImage"] == null)
            {
                ViewBag.Error = "Şifrelenmiş bir veri bulunamadı.";
                return View("Index");
            }

            var encryptedData = Convert.FromBase64String((string)TempData["EncryptedImage"]);
            var key = (byte[])TempData["Key"];

            try
            {
                var decryptedData = _encryptionModel.DecryptData(encryptedData, key);
                TempData["DecryptedImage"] = Convert.ToBase64String(decryptedData);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Çözme işlemi sırasında hata oluştu: {ex.Message}";
            }

            return View("Index");
        }
    }
}