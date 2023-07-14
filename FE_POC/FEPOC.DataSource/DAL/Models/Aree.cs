using System;
using System.Collections.Generic;

namespace FEPOC.DataSource.DAL.Models;

public partial class Aree
{
    public int Id { get; set; }

    public string Descr { get; set; } = null!;

    public int Idinsediamento { get; set; }

    public int? Idcalend { get; set; }

    public string Contapostidisp { get; set; } = null!;

    public int? Maxpostidisp { get; set; }

    public string? Tipo { get; set; }
}
