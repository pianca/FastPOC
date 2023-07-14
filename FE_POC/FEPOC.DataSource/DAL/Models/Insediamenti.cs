using System;
using System.Collections.Generic;

namespace FEPOC.DataSource.DAL.Models;

public partial class Insediamenti
{
    public int Id { get; set; }

    public string? Codice { get; set; }

    public string Descr { get; set; } = null!;

    public int? Idpolicyaccesso { get; set; }

    public int? Idportineria { get; set; }

    public int? Idcontrollovarchiconfig { get; set; }
}
