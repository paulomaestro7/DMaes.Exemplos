using DMaes.Exemplos.Autenticacao.Jwt.Identity.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace DMaes.Exemplos.Autenticacao.Jwt.Identity.Controllers
{
    [Route("imagem")]
    [ApiController]
    public class ImagemController : ControllerBase
    {
        [HttpPost]
        [Authorize("Bearer")]
        [Route("imagembase64")]
        public async Task<IActionResult> ImagemPost([FromBody] Foto Body)
        {
            try
            {
                var textBase64 = Convert.FromBase64String(Body.Base64);

                using (Image image = Image.FromStream(new MemoryStream(textBase64)))
                {
                    image.Save(@"Imagem\" + Body.NomeImagem + ".jpg", ImageFormat.Jpeg);
                }

                return Ok("Imagem recebida com sucesso!");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
