using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PCRemote.Core.Models;

public class RemoteRequest
{
    public int Version { get; set; } = 1;
    public long Timestamp { get; set; }
    public string Nonce { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string? Data { get; set; }
}