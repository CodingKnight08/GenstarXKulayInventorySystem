using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenstarXKulayInventorySystem.Shared.Helpers;

public class OrdersHelper
{
    public enum PurchaseRecieptOption
    {
        NonBIR,
        BIR
    }

    public enum PurchaseRecieveOption
    {
       RecieveAll,
       PartialRecieve,
       Pending,
    }

    public enum  PurchaseShipToOption
    {
        GeneralSantosCity,
        Polomolok,
        Warehouse,
    }
}
