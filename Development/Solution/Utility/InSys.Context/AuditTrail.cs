using InSys.ITI.Models.Models;
using LZStringCSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using z.Data;

namespace InSys.Context
{
    public class AuditTrail : DbContext
    {
        public DbSet<Audit> Audits { get; set; }
        public BrowserSession AuditSession { get; set; }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = base.SaveChanges(acceptAllChangesOnSuccess);
            OnAfterSaveChanges(auditEntries);
            return result;
        }
        public override int SaveChanges()
        {
            var auditEntries = OnBeforeSaveChanges();
            var result = base.SaveChanges(true);
            OnAfterSaveChanges(auditEntries);
            return result;
        }
        private List<AuditEntry> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<AuditEntry>();
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is Audit || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditEntry = new AuditEntry(entry);
                auditEntry.TableName = entry.Metadata.Relational().TableName;
                auditEntry.ID_User = AuditSession.ID_User;
                auditEntry.EntityState = entry.State;
                auditEntries.Add(auditEntry);

                var oldValues = entry.GetDatabaseValues();
                foreach (var property in entry.Properties)
                {
                    if (property.IsTemporary)
                    {
                        auditEntry.TemporaryProperties.Add(property);
                        continue;
                    }

                    string propertyName = property.Metadata.Name;
                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[propertyName] = property.CurrentValue;
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            auditEntry.OldValues[propertyName] = oldValues[propertyName];
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                if (oldValues[propertyName] == null && property.CurrentValue == null)
                                    continue;

                                if (oldValues[propertyName] == null ||
                                   property.CurrentValue == null ||
                                   !oldValues[propertyName].Equals(property.CurrentValue))
                                {
                                    auditEntry.OldValues[propertyName] = oldValues[propertyName];
                                    auditEntry.NewValues[propertyName] = property.CurrentValue;
                                }
                            }
                            break;
                    }
                }
            }
            return auditEntries;
        }

        private void OnAfterSaveChanges(List<AuditEntry> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count == 0) return;
            
            //for new record only
            foreach (var auditEntry in auditEntries.Where(x => !x.HasTemporaryProperties))
            {
                // Get the final value of the temporary properties
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey()) auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    else auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                }
                Audits.Add(auditEntry.ToAudit());
            }
            base.SaveChanges();
        }
    }
    [Table("tAudit")]
    public class Audit
    {
        public int ID { get; set; }
        public string TableName { get; set; }
        public DateTime DateTime { get; set; }
        public string KeyValues { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public int? ID_User { get; set; }
        public string EntityState { get; set; }
    }
    public class AuditEntry
    {
        public AuditEntry(EntityEntry entry)
        {
            Entry = entry;
        }
        public EntityEntry Entry { get; }
        public string TableName { get; set; }
        public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
        public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
        public List<PropertyEntry> TemporaryProperties { get; } = new List<PropertyEntry>();
        public int? ID_User { get; set; }
        public bool HasTemporaryProperties => TemporaryProperties.Any();
        public EntityState EntityState { get; set; }

        public Audit ToAudit()
        {
            var audit = new Audit();
            audit.TableName = TableName;
            audit.DateTime = DateTime.UtcNow;
            audit.KeyValues = JsonConvert.SerializeObject(KeyValues);
            audit.OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues);
            audit.NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues);
            audit.ID_User = ID_User;
            audit.EntityState = EntityState.ToString();
            return audit;
        }
    }
}
