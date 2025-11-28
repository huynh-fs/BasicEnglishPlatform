using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Services.DTOs
{
    public class AiQuizResponse
    {
        public string Question { get; set; } = string.Empty;
        public List<string> Options { get; set; } = null!;
        public string CorrectAnswer { get; set; } = string.Empty;
        public string Explanation { get; set; } = string.Empty;
    }
}
