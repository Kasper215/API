using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api")]
    public class AdmissionController : ControllerBase
    {
        private readonly AdmissionService _admissionService;
        private readonly HttpClient _httpClient;
        private readonly string _partnerApiUrl = "https://partner-api.amvera.io/api/validate";

        public AdmissionController(AdmissionService admissionService, IHttpClientFactory httpClientFactory)
        {
            _admissionService = admissionService;
            _httpClient = httpClientFactory.CreateClient();
        }

        /// <summary>
        /// Получение списка всех специальностей
        /// </summary>
        /// <returns>Список всех доступных специальностей и их проходные баллы</returns>
        [HttpGet("specialties")]
        [ProducesResponseType(typeof(List<Specialty>), 200)]
        public async Task<IActionResult> GetSpecialties()
        {
            var specialties = await _admissionService.GetSpecialtiesAsync();
            return Ok(specialties);
        }

        /// <summary>
        /// Получение списка всех абитуриентов
        /// </summary>
        /// <returns>Список всех зарегистрированных абитуриентов с расчетом баллов</returns>
        [HttpGet("applicants")]
        [ProducesResponseType(typeof(List<Applicant>), 200)]
        public async Task<IActionResult> GetApplicants()
        {
            var applicants = await _admissionService.GetApplicantsAsync();
            return Ok(applicants);
        }

        /// <summary>
        /// Регистрация нового абитуриента
        /// </summary>
        [HttpPost("applicants")]
        [ProducesResponseType(typeof(Applicant), 200)]
        public async Task<IActionResult> CreateApplicant([FromBody] Applicant request)
        {
            var applicant = await _admissionService.CreateApplicantAsync(request);
            return Ok(applicant);
        }

        /// <summary>
        /// Подача заявки на поступление
        /// </summary>
        /// <remarks>
        /// Пример запроса:
        /// 
        ///     POST /api/applications
        ///     {
        ///        "applicantId": 1,
        ///        "specialty1": "Информатика и вычислительная техника",
        ///        "specialty2": "Информационная безопасность"
        ///     }
        /// </remarks>
        /// <param name="request">Модель с ID абитуриента и двумя выбранными специальностями</param>
        /// <returns>Возвращает созданную заявку с результатом проверки (прошел / не прошел) и текстом уведомления.</returns>
        [HttpPost("applications")]
        [ProducesResponseType(typeof(Application), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> SubmitApplication([FromBody] CreateApplicationRequest request)
        {
            var applicants = await _admissionService.GetApplicantsAsync();
            var applicant = applicants.FirstOrDefault(a => a.Id == request.ApplicantId);
            
            if (applicant == null)
            {
                return NotFound(new { Message = "Абитуриент с таким Id не найден." });
            }

            // Шаг 2: Интеграция с API одногруппника
            var validationRequest = new
            {
                certificateScore = applicant.CertificateScore,
                russianScore = applicant.RussianScore,
                profileScore = applicant.SubjectScore
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync(_partnerApiUrl, validationRequest);
                if (response.IsSuccessStatusCode)
                {
                    var validationResult = await response.Content.ReadFromJsonAsync<ValidationResult>();
                    if (validationResult != null && !validationResult.IsValid)
                    {
                        return BadRequest(new { Message = validationResult.Message ?? "Баллы не прошли валидацию в партнёрской системе." });
                    }
                }
            }
            catch
            {
                // Игнорируем ошибку подключения к API одногруппника (чтобы система работала даже если его API недоступно)
            }

            // Шаг 3: Основная бизнес-логика (расчёт баллов, определение результата)
            var application = await _admissionService.ProcessApplicationAsync(request);
            
            return Ok(application);
        }

        /// <summary>
        /// Подтверждение поступления
        /// </summary>
        /// <param name="id">Уникальный идентификатор заявки (Application.Id)</param>
        /// <param name="request">Содержит флаг Confirm: true для зачисления, false для отказа</param>
        /// <returns>Статус (Вы зачислены / Вы не зачислены)</returns>
        [HttpPost("applications/{id}/confirm")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ConfirmApplication(int id, [FromBody] ConfirmApplicationRequest request)
        {
            var success = await _admissionService.ConfirmAdmissionAsync(id, request.Confirm);

            if (!success)
            {
                return NotFound(new { Message = "Заявка с таким Id или связанный абитуриент не найдены." });
            }

            string resultMessage = request.Confirm ? "Вы зачислены!" : "Вы не зачислены";
            return Ok(new { Message = resultMessage });
        }
    }
}
