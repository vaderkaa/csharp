namespace paczkomatoza;

public sealed record Sender(
    string Id,
    string Name,
    string PhoneNumber
);

public sealed record Recipient(
    string Id,
    string Name,
    string PhoneNumber
);

public enum DispatchMethod
{
    LockerDispatch,
    CourierPickup
}

public enum DeliveryMethod
{
    LockerDelivery,
    AddressDelivery
}

public enum ParcelSize
{
    Small,
    Medium,
    Large
}

public enum ShipmentStatus
{
    Prepared,
    InsertedIntoLocker,
    InTransit,
    WaitingInLockerToBeReceived,
    Delivered,
    Returned
}

public sealed record TrackingEvent(
    DateTime Timestamp,
    ShipmentStatus Status,
    string Location
);

public sealed class Courier
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string PhoneNumber { get; init; }
    public required string DeliveryArea { get; init; }
    public int MaxDailyShipments { get; init; } = 30;

    // Tracking codes are stored in assignment order and form the courier's manifest
    public List<string> AssignedTrackingCodes { get; } = [];
}

public sealed class Parcel
{
    public required string Id { get; init; }
    public decimal WeightKg { get; init; }

    // Parcels dropped off at a parcel locker have a declared size
    // For parcels picked up by a courier from an address, dimensions are used
    public ParcelSize? DeclaredSize { get; init; }
    public decimal? LengthCm { get; init; }
    public decimal? WidthCm { get; init; }
    public decimal? HeightCm { get; init; }
}

public sealed class Locker
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Address { get; init; }
    public required string City { get; init; }

    public Dictionary<ParcelSize, int> CapacityBySize { get; init; } = new()
    {
        [ParcelSize.Small] = 10,
        [ParcelSize.Medium] = 6,
        [ParcelSize.Large] = 3
    };

    public Dictionary<ParcelSize, HashSet<string>> StoredShipmentsBySize { get; } =
        Enum.GetValues<ParcelSize>().ToDictionary(size => size, _ => new HashSet<string>(StringComparer.Ordinal));

    public int Occupied(ParcelSize size) => StoredShipmentsBySize[size].Count;

    public int Available(ParcelSize size) => CapacityBySize[size] - Occupied(size);

    public bool TryStore(string trackingCode, ParcelSize size)
    {
        var stored = StoredShipmentsBySize[size];
        if (stored.Count >= CapacityBySize[size])
        {
            return false;
        }
        return stored.Add(trackingCode);
    }

    public bool Remove(string trackingCode, ParcelSize size) => StoredShipmentsBySize[size].Remove(trackingCode);
}

public sealed class ShipmentOrder
{
    public required string TrackingCode { get; init; }
    public required string SenderId { get; init; }
    public required string RecipientId { get; init; }
    public required Parcel Parcel { get; init; }

    public DispatchMethod DispatchMethod { get; init; }
    public string? SourceLockerId { get; init; }
    public string? SourceAddress { get; init; }

    public DeliveryMethod DeliveryMethod { get; init; }
    public string? DestinationLockerId { get; init; }
    public string? DestinationAddress { get; init; }
    public required string DestinationCity { get; init; }

    public DateTime CreatedAt { get; init; }
    public ShipmentStatus Status { get; set; }
    public string? DeliveryCourierId { get; set; }
    public DateOnly? PlannedDeliveryDate { get; set; }
    public DateTime? PickupDeadline { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

public sealed class DistributionCenter
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string City { get; init; }

    public Dictionary<string, ShipmentOrder> Shipments { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, Sender> Senders { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, Recipient> Recipients { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, Courier> Couriers { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, Locker> Lockers { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, List<TrackingEvent>> TrackingHistory { get; } = new(StringComparer.Ordinal);

    // Shipments that have arrived at the center and are awaiting assignment of a delivery courier
    public Queue<string> ShipmentsAwaitingAssignment { get; } = new();

    public ShipmentOrder GetShipmentOrThrow(string trackingCode) =>
        Shipments.TryGetValue(trackingCode, out var shipment)
            ? shipment
            : throw new KeyNotFoundException($"Nie znaleziono przesyłki '{trackingCode}'.");

    public static DistributionCenter GetDefaultDistributionCenter()
    {
        var dc = new DistributionCenter { Id = "1", Name = "Warszawa1", City = "Warszawa" };

        Sender[] senders =
        [
            new Sender("1", "Tomek Kowalski", "660 100 000"),
            new Sender("2", "Jan Dzban", "660 100 001"),
            new Sender("3", "Adam Nowak", "660 100 002"),
            new Sender("4", "Kasia Kowalska", "660 100 003"),
            new Sender("5", "Piotr Witkowski", "660 100 004"),
        ];

        Recipient[] recipients =
        [
            new Recipient("1", "Marianna Nowak", "660 200 000"),
            new Recipient("2", "Janina Dzbanowa", "660 200 001"),
            new Recipient("3", "Urszula Kwiatek", "660 200 002"),
            new Recipient("4", "Paweł Zielony", "660 200 003"),
            new Recipient("5", "Tomek Żółtak", "660 100 004"),
            new Recipient("6", "Tomek Kowalski", "660 100 000"),
            new Recipient("7", "Jan Dzban", "660 100 001"),
        ];

        Courier[] couriers =
        [
            new Courier { Id = "1", Name = "Vasili Adamchik", PhoneNumber = "700 123 200", DeliveryArea = "Area1", MaxDailyShipments = 10 },
            new Courier { Id = "2", Name = "Ivan Bregovich", PhoneNumber = "700 123 201", DeliveryArea = "Area2", MaxDailyShipments = 20 },
            new Courier { Id = "3", Name = "Agnieszka Jakubowska", PhoneNumber = "700 123 202", DeliveryArea = "Area1", MaxDailyShipments = 30 }
        ];

        Parcel[] parcels =
        [
            new Parcel { Id = "1", WeightKg = 0.1m, DeclaredSize = ParcelSize.Small, LengthCm = 10, WidthCm = 10, HeightCm = 5 },
            new Parcel { Id = "2", WeightKg = 0.2m, DeclaredSize = ParcelSize.Small, LengthCm = 20, WidthCm = 10, HeightCm = 2 },
            new Parcel { Id = "3", WeightKg = 0.5m, DeclaredSize = ParcelSize.Medium, LengthCm = 10, WidthCm = 7, HeightCm = 10 },
            new Parcel { Id = "4", WeightKg = 1.1m, DeclaredSize = ParcelSize.Large, LengthCm = 30, WidthCm = 5, HeightCm = 25 },
            new Parcel { Id = "5", WeightKg = 2.1m, DeclaredSize = ParcelSize.Small, LengthCm = 40, WidthCm = 12, HeightCm = 5 },
        ];

        Locker[] lockers =
        [
            new Locker { Id = "1", Name = "WAW01", Address = "Marszałkowska 10", City = "Warszawa" },
            new Locker { Id = "2", Name = "WAW02", Address = "Plac Konstytucji 1", City = "Warszawa" },
            new Locker { Id = "3", Name = "OTW01", Address = "Kupiecka 10", City = "Otwock" },
            new Locker { Id = "4", Name = "GDA42", Address = "Hallera 1", City = "Gdansk" },
            new Locker { Id = "5", Name = "SPT10", Address = "Sopot Molo 1", City = "Sopot" },
        ];

        ShipmentOrder[] shipmentOrders =
        [
            // 1. Już dostarczona przez kuriera
            new ShipmentOrder
            {
                TrackingCode = "0000000000000000001",
                SenderId = "1",
                RecipientId = "2",
                Parcel = parcels[0],
                DispatchMethod = DispatchMethod.CourierPickup,
                SourceAddress = "Marszałkowska 1, Warszawa",
                DeliveryMethod = DeliveryMethod.AddressDelivery,
                DestinationAddress = "Puławska 15, Warszawa",
                DestinationCity = "Warszawa",
                CreatedAt = new DateTime(2026, 01, 01, 12, 00, 00),
                Status = ShipmentStatus.Delivered,
                DeliveryCourierId = "1",
                PlannedDeliveryDate = new DateOnly(2026, 01, 03),
                DeliveredAt = new DateTime(2026, 01, 03, 14, 30, 00)
            },

            // 2. W drodze (przypisana do kuriera 2)
            new ShipmentOrder
            {
                TrackingCode = "0000000000000000002",
                SenderId = "1",
                RecipientId = "3",
                Parcel = parcels[1],
                DispatchMethod = DispatchMethod.CourierPickup,
                SourceAddress = "Mokotowska 5, Warszawa",
                DeliveryMethod = DeliveryMethod.AddressDelivery,
                DestinationAddress = "Aleje Jerozolimskie 20, Warszawa",
                DestinationCity = "Warszawa",
                CreatedAt = new DateTime(2026, 02, 01, 10, 15, 00),
                Status = ShipmentStatus.InTransit,
                DeliveryCourierId = "2",
                PlannedDeliveryDate = new DateOnly(2026, 02, 03)
            },

            // 3. Włożona przez nadawcę do paczkomatu WAW01 (czeka na odbiór przez kuriera)
            new ShipmentOrder
            {
                TrackingCode = "0000000000000000003",
                SenderId = "2",
                RecipientId = "1",
                Parcel = parcels[2],
                DispatchMethod = DispatchMethod.LockerDispatch,
                SourceLockerId = "1",
                DeliveryMethod = DeliveryMethod.LockerDelivery,
                DestinationLockerId = "2",
                DestinationCity = "Warszawa",
                CreatedAt = new DateTime(2026, 03, 01, 09, 00, 00),
                Status = ShipmentStatus.InsertedIntoLocker,
                PlannedDeliveryDate = new DateOnly(2026, 03, 03)
            },

            // 4. Czeka w paczkomacie docelowym WAW02 na odbiór przez odbiorcę
            new ShipmentOrder
            {
                TrackingCode = "0000000000000000004",
                SenderId = "3",
                RecipientId = "4",
                Parcel = parcels[3],
                DispatchMethod = DispatchMethod.LockerDispatch,
                SourceLockerId = "3",
                DeliveryMethod = DeliveryMethod.LockerDelivery,
                DestinationLockerId = "2",
                DestinationCity = "Warszawa",
                CreatedAt = new DateTime(2026, 03, 05, 11, 20, 00),
                Status = ShipmentStatus.WaitingInLockerToBeReceived,
                DeliveryCourierId = "3",
                PlannedDeliveryDate = new DateOnly(2026, 03, 07),
                PickupDeadline = new DateTime(2026, 03, 09, 11, 20, 00)
            },

            // 5. W sortowni, czeka w kolejce na przypisanie kuriera
            new ShipmentOrder
            {
                TrackingCode = "0000000000000000005",
                SenderId = "4",
                RecipientId = "5",
                Parcel = parcels[4],
                DispatchMethod = DispatchMethod.CourierPickup,
                SourceAddress = "Długa 8, Gdańsk",
                DeliveryMethod = DeliveryMethod.LockerDelivery,
                DestinationLockerId = "4",
                DestinationCity = "Gdansk",
                CreatedAt = new DateTime(2026, 03, 10, 16, 45, 00),
                Status = ShipmentStatus.InTransit,
                PlannedDeliveryDate = new DateOnly(2026, 03, 12)
            }
        ];

        foreach (var s in senders) dc.Senders.Add(s.Id, s);
        foreach (var r in recipients) dc.Recipients.Add(r.Id, r);
        foreach (var c in couriers) dc.Couriers.Add(c.Id, c);
        foreach (var l in lockers) dc.Lockers.Add(l.Id, l);

        foreach (var so in shipmentOrders)
        {
            dc.Shipments.Add(so.TrackingCode, so);
            dc.TrackingHistory[so.TrackingCode] =
            [
                new TrackingEvent(so.CreatedAt, ShipmentStatus.Prepared, "System")
            ];
        }

        // Synchronizacja powiązań:
        // 1. Przypisanie przesyłek do manifestów kurierów
        dc.Couriers["1"].AssignedTrackingCodes.Add("0000000000000000001");
        dc.Couriers["2"].AssignedTrackingCodes.Add("0000000000000000002");
        dc.Couriers["3"].AssignedTrackingCodes.Add("0000000000000000004");

        // 2. Fizyczne umieszczenie paczek w skrytkach paczkomatów
        dc.Lockers["1"].TryStore("0000000000000000003", ParcelSize.Medium);
        dc.Lockers["2"].TryStore("0000000000000000004", ParcelSize.Large);

        // 3. Dodanie przesyłki nr 5 do kolejki oczekującej na kuriera
        dc.ShipmentsAwaitingAssignment.Enqueue("0000000000000000005");

        // 4. Uzupełnienie historii śledzenia
        dc.TrackingHistory["0000000000000000001"].Add(new TrackingEvent(new DateTime(2026, 01, 03, 14, 30, 00), ShipmentStatus.Delivered, "Puławska 15, Warszawa"));
        dc.TrackingHistory["0000000000000000002"].Add(new TrackingEvent(new DateTime(2026, 02, 02, 08, 00, 00), ShipmentStatus.InTransit, "Sortownia Warszawa1"));
        dc.TrackingHistory["0000000000000000003"].Add(new TrackingEvent(new DateTime(2026, 03, 01, 09, 15, 00), ShipmentStatus.InsertedIntoLocker, "WAW01"));
        dc.TrackingHistory["0000000000000000004"].Add(new TrackingEvent(new DateTime(2026, 03, 07, 11, 20, 00), ShipmentStatus.WaitingInLockerToBeReceived, "WAW02"));
        dc.TrackingHistory["0000000000000000005"].Add(new TrackingEvent(new DateTime(2026, 03, 11, 06, 00, 00), ShipmentStatus.InTransit, "Sortownia Warszawa1"));

        return dc;
    }
}