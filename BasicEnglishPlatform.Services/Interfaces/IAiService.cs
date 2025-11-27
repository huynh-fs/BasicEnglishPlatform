using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicEnglishPlatform.Services.Interfaces
{
    public interface IAiService
    {
        Task<string> AskAiAsync(string question);
    }
}
