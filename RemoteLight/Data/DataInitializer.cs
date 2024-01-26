using Microsoft.EntityFrameworkCore;
using RemoteLight.Models;

namespace RemoteLight.Data
{
	public static class DataInitializer
	{
		public static void SeedDatabase(this ModelBuilder modelBuilder)
		{

			CardOwner cardOwner = new()
			{
				Id = 1,
				Name = "some user",
			};

			CardOwner cardOwner2 = new()
			{
				Id = 2,
				Name = "nice",
			};

			RFIDCard card = new()
			{
				Id = "687777954811",
				FkCardOwnerId = cardOwner.Id
			};

			RFIDCard card2 = new()
			{
				Id = "1022787718182",
				FkCardOwnerId = cardOwner2.Id
			};

			Broker broker = new()
			{
				BrokerId = 1,
				IPAddress = "test.mosquitto.org",
				Port = 1883
			};

			Room room = new()
			{
				Id = 1,
				FkBrokerId = broker.BrokerId,
				AdditionalInformation = "Raspberry pi"
			};

			Access access = new()
			{
				Id = 1,
				FkRFIDCardId = card.Id,
				FkRoomId = room.Id,
			};


			modelBuilder.Entity<CardOwner>().HasData(cardOwner, cardOwner2);
			modelBuilder.Entity<RFIDCard>().HasData(card, card2);
			modelBuilder.Entity<Broker>().HasData(broker);
			modelBuilder.Entity<Room>().HasData(room);
			modelBuilder.Entity<Access>().HasData(access);

		}
	}
}
