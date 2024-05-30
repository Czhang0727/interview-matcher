using Azure;
using Azure.Data.Tables;

public interface IAzureStorageClient
{
    string Test();
    UserEntity AddUser(string userId, string userName);
    Dictionary<string, string> MatchGroup();
    void AddNewTimeSlot();
}

public class UserEntity : ITableEntity
{
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public required string UserName { get; set; }
    public required string UserId { get; set; }
}

public class TimeSlotEntity : ITableEntity
{
    public required string PartitionKey { get; set; }
    public required string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public required string TimeSlot { get; set; }
}

public class AzureStorageClient : IAzureStorageClient
{
    private TableClient asClient;
    public AzureStorageClient()
    {
        this.asClient = new TableClient(new Uri("http://127.0.0.1:10002/devstoreaccount1"), "Users", new TableSharedKeyCredential("devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="));
    }
    public string Test()
    {
        UserEntity user = this.asClient.Query<UserEntity>(user => user.UserName == "Chenyi").First();
        Console.WriteLine(user.PartitionKey, user.RowKey, user.UserName);
        return user.UserId;
    }

    public UserEntity AddUser(string userId, string userName)
    {
        TableClient timeSlotTableClient = new TableClient(new Uri("http://127.0.0.1:10002/devstoreaccount1"), "TimeSlots", new TableSharedKeyCredential("devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="));
        Pageable<TimeSlotEntity> entities = timeSlotTableClient.Query<TimeSlotEntity>(ent => true);
        if (entities != null && entities.Count() > 0)
        {
            TimeSlotEntity currentTimeSlot = entities.MaxBy(slot => slot.TimeSlot);
            if (currentTimeSlot != null)
            {
                UserEntity user = new UserEntity() { PartitionKey = currentTimeSlot.TimeSlot, RowKey = userId, UserId = userId, UserName = userName };
                this.asClient.AddEntity(user);
                return user;
            }

        }
        throw new Exception("No TimeSlot Found");
    }

    public void AddNewTimeSlot()
    {
        TableClient timeSlotTableClient = new TableClient(new Uri("http://127.0.0.1:10002/devstoreaccount1"), "TimeSlots", new TableSharedKeyCredential("devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="));
        Pageable<TimeSlotEntity> slot = timeSlotTableClient.Query<TimeSlotEntity>(ent => true);
        int newTimeSlot = 0;
        if (slot.Count() > 0)
        {
            newTimeSlot = Int32.Parse(slot.First().TimeSlot) + 1;
        }
        timeSlotTableClient.AddEntity<TimeSlotEntity>(new TimeSlotEntity()
        {
            PartitionKey = "timeslot",
            RowKey = newTimeSlot.ToString(),
            TimeSlot = newTimeSlot.ToString()
        });
    }

    public Dictionary<string, string> MatchGroup()
    {
        TableClient timeSlotTableClient = new TableClient(new Uri("http://127.0.0.1:10002/devstoreaccount1"), "TimeSlots", new TableSharedKeyCredential("devstoreaccount1", "Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw=="));
        Pageable<TimeSlotEntity> slot = timeSlotTableClient.Query<TimeSlotEntity>(ent => true);

        string currentTimeSlot = slot.MaxBy(slot => Int32.Parse(slot.TimeSlot)).TimeSlot;
        List<UserEntity> entities = this.asClient.Query<UserEntity>(user => user.PartitionKey == currentTimeSlot).ToList();

        Dictionary<string, string> matchDict = new Dictionary<string, string>();
        Random rnd = new Random();

        if (entities.Count() % 2 == 1)
        {
            int indexToRemove = rnd.Next(0, entities.Count());
            entities.RemoveAt(indexToRemove);
        }

        entities = entities.OrderBy(x => Random.Shared.Next()).ToList();
        int point = 0;
        while (point <= (entities.Count() / 2) && entities.Count() > 0)
        {
            matchDict.Add(entities[point].UserName, entities[entities.Count() - point - 1].UserName);
            point += 1;
        }

        return matchDict;
    }
}