using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aruba.CmpService.BaremetalProvider.Abstractions.Interfaces.Jobs;
public interface IDeleteTokensJob
{
    /// <summary>
    /// Job for deleting tokens,
    /// </summary>
    /// <returns></returns>
    void Execute();
}
