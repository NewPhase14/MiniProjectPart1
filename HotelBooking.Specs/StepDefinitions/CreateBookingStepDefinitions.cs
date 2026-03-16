using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotelBooking.Core;
using Moq;
using Reqnroll;
using Xunit;

namespace HotelBooking.Specs.StepDefinitions;

[Binding]
public sealed class CreateBookingStepDefinitions
{
    private DateTime _startDate;
    private DateTime _endDate;
    private bool _result;
    
    private IBookingManager bookingManager;
    private Mock<IRepository<Booking>> mockBookingRepository;
    private Mock<IRepository<Room>> mockRoomRepository;

    public CreateBookingStepDefinitions(){
        mockBookingRepository = new Mock<IRepository<Booking>>();
        mockRoomRepository = new Mock<IRepository<Room>>();
            
        var rooms = new List<Room>
        {
            new Room { Id = 1, Description = "Room A" },
        };
        mockRoomRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(rooms);
            
        DateTime startBookingA = DateTime.Today.AddDays(10);
        DateTime endBookingA = DateTime.Today.AddDays(20);
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
            }
        };
        mockBookingRepository.Setup(b => b.GetAllAsync()).ReturnsAsync(bookings);
            
        bookingManager = new BookingManager(mockBookingRepository.Object, mockRoomRepository.Object);
    }
    
    [Given("the start date is (.*)")]
    public void GivenTheStartDateIs(DateTime startDate)
    {
        _startDate = startDate;
    }

    [Given("the end date is (.*)")]
    public void GivenTheEndDateIs(DateTime endDate)
    {
        _endDate = endDate;
    }

    [When("I press book")]
    public void WhenIPressBook()
    {
        var booking = new Booking
            {
                CustomerId= 1,
                StartDate = _startDate,
                EndDate =  _endDate,
            };



        var result = bookingManager.CreateBooking(booking);
        _result = result.Result;
    }

    [Then("the result should be (.*)")]
    public void ThenTheResultShouldBe(bool result)
    {
        Assert.Equal(result, _result);
    }

  
}