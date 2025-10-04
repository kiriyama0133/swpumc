using System;
using System.IO;
using System.Linq;

namespace swpumc.Services
{
    public class BackgroundService : IBackgroundService
    {
        private readonly string _carouselPath;
        private string _currentBackgroundPath = string.Empty;
        private readonly Random _random;
        
        public string CurrentBackgroundPath => _currentBackgroundPath;
        
        public event EventHandler<string>? BackgroundChanged;
        
        public BackgroundService()
        {
            _carouselPath = "/Assets/";
            _random = new Random();
            SelectRandomBackground();
        }
        
        public void SelectRandomBackground()
        {
            var backgrounds = GetAvailableBackgrounds();
            if (backgrounds.Length > 0)
            {
                var randomIndex = _random.Next(backgrounds.Length);
                _currentBackgroundPath = backgrounds[randomIndex];
                Console.WriteLine($"[BackgroundService] Selected background: {_currentBackgroundPath}");
                BackgroundChanged?.Invoke(this, _currentBackgroundPath);
            }
        }
        
        public string[] GetAvailableBackgrounds()
        {
            return new[]
            {
                $"{_carouselPath}1.png",
                $"{_carouselPath}2.png",
                $"{_carouselPath}3.png"
            };
        }
    }
}
