using System;
using System.Collections.Generic;

namespace FileEncryptionWebApp.DataAccess.EF.Models;

public partial class EncryptionKey
{
    public int EncryptionKeyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public byte[] Nonce { get; set; } = null!;

    public byte[] EncryptionKey1 { get; set; } = null!;

    public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();
}
