using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using ZoomIntegrationApp.Models;
using ZoomIntegrationApp.Services;

namespace ZoomIntegrationApp.Controllers
{
    public class MeetingController : Controller
    {
        #region
        private readonly IOptions<Zoom> _options;
        private readonly IMeetingService _meetingService;

        public MeetingController(
            IOptions<Zoom> options,
            IMeetingService meetingService)
        {
            _options = options;
            _meetingService = meetingService;
        }
        #endregion
        public IActionResult SignIn()
        {
            var Url = string.Format(_options.Value.AuthorizationUrl, _options.Value.ClientID, _options.Value.RedirectUrl);
            return Redirect(Url);
        }
        public async Task<IActionResult> zoom(string code)
        {
            await _meetingService.AuthenticationSuccess(code);
            return RedirectToAction("Index");
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> GetUserDetails()
        {
            var user = await _meetingService.GetUserDetails();
            return View(user);
        }
        public async Task<IActionResult> RefreshToken()
        {
            await _meetingService.RefreshToken();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> CreateMeeting()
        {
            await _meetingService.CreateMeeting();
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> GetAll()
        {
            var list = await _meetingService.GetAll();
            return View(list);
        }
        public async Task<IActionResult> GetByID(string ID)
        {
            await _meetingService.GetMeetingByID(ID);
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(string ID)
        {
            await _meetingService.DeleteMeeting(ID);
            return RedirectToAction("GetAll");
        }

    }
}
