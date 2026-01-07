using System;
using System.Collections.Generic;

namespace FileEncryptionWebApp.DataAccess.EF.Models;

public partial class Entry
{
    public int EntryId { get; set; }

    public string Entry1 { get; set; } = null!;

    public DateTime CreationDate { get; set; }

    public bool IsEncrypted { get; set; }

    public int EncryptionKeyId { get; set; }

    public virtual EncryptionKey EncryptionKey { get; set; } = null!;
}
