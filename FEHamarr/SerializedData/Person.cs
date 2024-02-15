using FEHamarr.FEHArchive;
using NPOI.OpenXml4Net.OPC.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace FEHamarr.SerializedData
{
    public interface IHSDStruct
    {

    }

    public interface IPointedStruct<T>
    {
        T Read(FEHArcReader reader);
    }

    

    public interface IPointedList<T>
    {
        ulong count { get; set; }
        T Read(FEHArcReader reader, ulong count);
    }
}
