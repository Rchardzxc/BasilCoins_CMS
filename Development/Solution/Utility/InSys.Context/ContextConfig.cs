using System;
using System.Collections.Generic;
using System.Text;

namespace InSys.Context
{
    public class ContextConfig
    {
        public static string SQLConnection { get; private set; }
        public static string KioskConnection { get; private set; }
        public static List<EntityModel> EntityModels { get; set; } = new List<EntityModel>();

        public static void AddSQLConnection(string sqlConnection)
        {
            SQLConnection = sqlConnection;
        }
        public static void AddKioskConnection(string _KioskConnection)
        {
            KioskConnection = _KioskConnection;
        }

        public static void AddEntityModel(Type entity, int entityType)
        {
            EntityModels.Add(new EntityModel() { Entity = entity, Type = entityType });
        }

        public class EntityModel
        {
            public Type Entity { get; set; }
            public int Type { get; set; }
        }
    }
}
