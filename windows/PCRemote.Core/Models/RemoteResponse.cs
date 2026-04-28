using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCRemote.Core.Models;

public class RemoteResponse
{
    public bool Ok { get; set; }

    public string? Message { get; set; }

    public string? ErrorCode { get; set; }

    public object? Data { get; set; }
}
