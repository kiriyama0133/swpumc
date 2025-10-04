using System;

namespace swpumc.Services
{
    public interface ITrayService
    {
        void Initialize();
        void Show();
        void Hide();
        void ShowWindow();
        void HideWindow();
        void Exit();
    }
}
