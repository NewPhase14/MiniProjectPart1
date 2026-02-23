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
            DateTime startBookingB = DateTime.Today.AddDays(10);
            DateTime endBookingB = DateTime.Today.AddDays(19);
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

        [Fact]
        public async Task FindAvailableRoom_NoRoomsAvailable_ReturnsMinusOne()
        {
            // Arrange
            DateTime date = DateTime.Today.AddDays(10);
            // Act
            int roomId = await bookingManager.FindAvailableRoom(date, date);
            // Assert
            Assert.Equal(-1, roomId);
        }
        
        [Fact]
        public async Task CreateBooking_CantCreateBooking_ReturnsFalse()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(10);
            DateTime endDate = DateTime.Today.AddDays(20);
            Booking booking = new Booking { StartDate = startDate, EndDate = endDate, CustomerId = 1 };

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CreateBooking_CanCreateBooking_ReturnsTrue()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(2);
            Booking booking = new Booking { StartDate = startDate, EndDate = endDate, CustomerId = 1 };

            // Act
            bool result = await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(result);
        }
        
        [Fact]
        public async Task CreateBooking_WhenSuccessfullyCreated_SetsIsActiveToTrue()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(2);
            Booking booking = new Booking { StartDate = startDate, EndDate = endDate, CustomerId = 1 };

            // Act
            await bookingManager.CreateBooking(booking);

            // Assert
            Assert.True(booking.IsActive);
        }
        
        

        [Fact]
        public async Task GetFullyOccupiedDates_Between1And30_Returns11Days()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(30);
            
            // Act
            List<DateTime> fullyOccupiedDates = await bookingManager.GetFullyOccupiedDates(startDate, endDate);
            
            // Assert
            var expected = Enumerable.Range(10, 10)
                .Select(i => DateTime.Today.AddDays(i).Date)
                .ToList();

            Assert.Equal(expected, fullyOccupiedDates.Select(d => d.Date).ToList());
        }

        [Fact]
        public async Task GetFullyOccupiedDates_Between1And2_ReturnsEmptyList()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(1);
            DateTime endDate = DateTime.Today.AddDays(2);

            // Act
            List<DateTime> fullyOccupiedDates = await bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            Assert.Empty(fullyOccupiedDates);

        }
        
        [Fact]
        public async Task GetFullyOccupiedDates_StartDateAfterEndDate_ThrowsArgumentException()
        {
            // Arrange
            DateTime startDate = DateTime.Today.AddDays(10);
            DateTime endDate = DateTime.Today.AddDays(5);

            // Act
            Task result() => bookingManager.GetFullyOccupiedDates(startDate, endDate);

            // Assert
            await Assert.ThrowsAsync<ArgumentException>(result);
        }
    }
}
