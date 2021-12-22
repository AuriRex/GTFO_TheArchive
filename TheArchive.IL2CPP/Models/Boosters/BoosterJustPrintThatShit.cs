using DropServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;

namespace TheArchive.Models.Boosters
{
    public class BoosterJustPrintThatShit
    {
#warning TODO
        /*public static string GetJSON(DropServer.BoosterImplantPlayerData data)
        {

            var cd = new CustomBoosterImplantPlayerData(data);

            string json = JsonConvert.SerializeObject(cd, Formatting.Indented);

            return json;
        }

        public static DropServer.BoosterImplantPlayerData Test(CustomBoosterImplantPlayerData data)
        {
            return data.ToBaseGame();
        }*/

        public static string Transaction(CustomBoosterTransaction transaction)
        {
            if (transaction == null) return "[!! Transaction was empty !!]";

            string str = string.Empty;

            str += $"AcknowledgeIds: [{(transaction.AcknowledgeIds != null ? string.Join(", ", transaction.AcknowledgeIds.ToArray()) : "null")}], ";
            str += $"Missed: [M:{transaction.AcknowledgeMissed?.Basic}, B:{transaction.AcknowledgeMissed?.Advanced}, A:{transaction.AcknowledgeMissed?.Specialized}], ";
            str += $"DropIds: [{(transaction.DropIds != null ? string.Join(", ", transaction.DropIds.ToArray()) : "null")}], ";
            str += $"TouchIds: [{(transaction.TouchIds != null ? string.Join(", ", transaction.TouchIds.ToArray()) : "null")}], ";
            str += $"MaxBackendTemplateId: {transaction.MaxBackendTemplateId}";

            return str;
        }
    }
}
