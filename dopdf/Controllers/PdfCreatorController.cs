using DinkToPdf;
using DinkToPdf.Contracts;
using dopdf.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace dopdf.Controllers
{
    [Route("api/pdfcreator")]
    [ApiController]
    public class PdfCreatorController : ControllerBase
    {
        private IConverter _converter;
        private readonly ILoggerManager _logger;
        private readonly IConfiguration _config;
        public PdfCreatorController(IConverter converter, ILoggerManager logger, IConfiguration config)
        {
            _converter = converter;
            _logger = logger;
            _config = config;
        }

        [HttpPost]
        public IActionResult CreatePDF([FromBody] dynamic json)
        {
            try
            {
                var converter = new ExpandoObjectConverter();
                dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(json.ToString(), converter);
                var globalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10 },
                    DocumentTitle = "PDF Document",
                    Out = Path.Combine(_config.GetValue<string>("Custom:outFolder"), obj.fileName + ".pdf")
                };

                var objectSettings = new ObjectSettings
                {
                    PagesCount = true,
                    HtmlContent = TemplateGenerator.PatchToTmpl(obj.data, Path.Combine(_config.GetValue<string>("Custom:tmplFolder"), obj.tmplName + ".html")),
                    WebSettings = { DefaultEncoding = "utf-8"/*, UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "assets", "styles.css")*/ },
                    HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Стр. [page] из [toPage]", Line = true },
                    FooterSettings = { FontName = "Arial", FontSize = 9, Line = true, Center = "Report Footer" }
                };

                var pdf = new HtmlToPdfDocument()
                {
                    GlobalSettings = globalSettings,
                    Objects = { objectSettings }
                };

                _converter.Convert(pdf);
                return Ok(operationResult.success());
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong in the {nameof(CreatePDF)} action {ex}");
                return Ok(operationResult.failure().addError(ex.GetBaseException().Message));
            }
        }
    }
}
