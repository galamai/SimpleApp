using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleApp.Storage
{
    public interface IHasETag
    {
        string ETag { get; set; }
    }
}
