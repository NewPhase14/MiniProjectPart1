using System;
using HotelBooking.Core;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;


namespace HotelBooking.UnitTests
{
    public class BookingManagerTests
    {
        private IBookingManager bookingManager;
        private Mock<IRepository<Booking>> mockBookingRepository;
        private Mock<IRepository<Room>> mockRoomRepository;

        public BookingManagerTests(){
            mockBookingRepository = new Mock<IRepository<Booking>>();
            mockRoomRepository = new Mock<IRepository<Room>>();
            
            var rooms = new List<Room>
            {
                new Room { Id = 1, Description = "Room A" },
                new Room { Id = 2, Description = "Room B" }
            };
            mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            
            DateTime startBookingA = DateTime.Today.AddDays(10);
            DateTime endBookingA = DateTime.Today.AddDays(20);
            DateTime startBookingB = DateTime.Today.AddDays(30);
            DateTime endBookingB = DateTime.Today.AddDays(40);
            var bookings = new List<Booking>
            {
                new Booking 
                { 
                    Id = 1, 
                    RoomId = 1, 
                    StartDate = startBookingA, 
                    EndDate = endBookingA, 
                    IsActive = true, 
                    CustomerId = 1 
                },
                new Booking
                {
                    Id = 2,
                    RoomId = 2,
                    StartDate =  startBookingB,
                    EndDate = endBookingB,
                    IsActive = true,
                    CustomerId = 2
                }
            };
            mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);
            
            bookingManager = new BookingManager(mockBookingRepository.Object, mockRoomRepository.Object);
        }

     
        [Fact]
        public async Task FindAvailableRoom_StartDateNotInTheFuture_ThrowsArgumentException()
        {
            // Arrange
            DateTime date = DateTime.Today;

            // Act
            Task result() => bookingManager.FindAvailableRoom(date, date);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_RoomIdNotMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.NotEqual(-1, roomId);
        }

        [Fact]
        public async Task FindAvailableRoom_RoomAvailable_ReturnsAvailableRoom()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(1);
            
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);

            var bookingForReturnedRoomId = (await mockBookingRepository.Object.GetAllAsync()).
                Where(b => b.RoomId == roomId
                           && b.StartDate <= date
                           && b.EndDate >= date
                           && b.IsActive);
            
            // Assert
            Assert.Empty(bookingForReturnedRoomId);
        }
        
    }
}
