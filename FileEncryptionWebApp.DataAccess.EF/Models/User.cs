using System;
using System.Collections.Generic;

namespace FileEncryptionWebApp.DataAccess.EF.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? EncryptionKeyId { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public virtual EncryptionKey? EncryptionKey { get; set; }

    public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();
}
