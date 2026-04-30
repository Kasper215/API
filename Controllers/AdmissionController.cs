using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api")]
    public class AdmissionController : ControllerBase
    {
        private readonly AdmissionService _admissionService;

        public AdmissionController(AdmissionService admissionService)
        {
            _admissionService = admissionService;
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
        [ProducesResponseType(404)]
        public async Task<IActionResult> SubmitApplication([FromBody] CreateApplicationRequest request)
        {
            var application = await _admissionService.ProcessApplicationAsync(request);
            
            if (application == null)
            {
                return NotFound(new { Message = "Абитуриент с таким Id не найден." });
            }

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
