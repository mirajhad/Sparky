﻿using Bongo.Core.Services;
using Bongo.DataAccess.Repository.IRepository;
using Bongo.Models.Model;
using Bongo.Models.Model.VM;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bongo.Core
{
    [TestFixture]
    public class StudyRoomBookingServiceTests
    {
        private StudyRoomBooking _request;
        private List<StudyRoom> _availableStudyRoom;
        private Mock<IStudyRoomBookingRepository> _studyRoomBookingRepoMock;
        private Mock<IStudyRoomRepository> _studyRoomRepoMock;
        private StudyRoomBookingService _bookingService;

        [SetUp]
        public void Setup()
        {
            _request = new StudyRoomBooking
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Date = new DateTime(2022, 1, 1)
            };

            _availableStudyRoom = new List<StudyRoom>
            {
                new StudyRoom {
                Id = 10, RoomName="Michingan", RoomNumber ="A202"
                }
            };


            _studyRoomBookingRepoMock = new Mock<IStudyRoomBookingRepository>();
            _studyRoomRepoMock = new Mock<IStudyRoomRepository>();
            _studyRoomRepoMock.Setup(x => x.GetAll()).Returns(_availableStudyRoom);
            _bookingService = new StudyRoomBookingService(
                _studyRoomBookingRepoMock.Object,
                _studyRoomRepoMock.Object
                );
        }

        [TestCase]
        public void GetAllBooking_InvokedMethod_CheckIfRepoIsCalled()
        {
            _bookingService.GetAllBooking();
            _studyRoomBookingRepoMock.Verify(repo => repo.GetAll(null), Times.Once);
        }

        [TestCase]
        public void BookingException_NullRequest_ThrowsException()
        {
            var exception = ClassicAssert.Throws<ArgumentNullException>(
                ()=> _bookingService.BookStudyRoom(null) );
            //ClassicAssert.AreEqual("Value cannot be null. (Parameter 'request')", exception.Message);
            ClassicAssert.AreEqual("request", exception.ParamName);
        }

        [Test]
        public void StudyRoomBooking_SaveBookingWithAvailableRoom_ReturnsResultWithAllValues() 
        {
            StudyRoomBooking savedStudyRoomBooking = null;
            _studyRoomBookingRepoMock.Setup(x=> x.Book(It.IsAny<StudyRoomBooking>()))
                .Callback<StudyRoomBooking>(b => savedStudyRoomBooking = b);

            //act
            _bookingService.BookStudyRoom(_request);

            //assert
            _studyRoomBookingRepoMock.Verify(x=>x.Book(It.IsAny<StudyRoomBooking>()), Times.Once);

            ClassicAssert.NotNull(savedStudyRoomBooking);
            ClassicAssert.AreEqual(_request.FirstName, savedStudyRoomBooking.FirstName);
            ClassicAssert.AreEqual(_request.LastName, savedStudyRoomBooking.LastName);
            ClassicAssert.AreEqual(_request.Email, savedStudyRoomBooking.Email);
            ClassicAssert.AreEqual(_request.Date, savedStudyRoomBooking.Date);
            ClassicAssert.AreEqual(_availableStudyRoom.First().Id, savedStudyRoomBooking.StudyRoomId);
        }

        [Test]
        public void StudyRoomBookingResultCheck_InputRequest_ValuesMatchInRequest()
        {
            StudyRoomBookingResult result = _bookingService.BookStudyRoom(_request);

            ClassicAssert.NotNull(result);
            ClassicAssert.AreEqual(_request.FirstName, result.FirstName);
            ClassicAssert.AreEqual(_request.LastName, result.LastName);
            ClassicAssert.AreEqual(_request.Email, result.Email);
            ClassicAssert.AreEqual(_request.Date, result.Date);
        }

        [TestCase(true, ExpectedResult = StudyRoomBookingCode.Success)]
        [TestCase(false, ExpectedResult =StudyRoomBookingCode.NoRoomAvailable)]
        public StudyRoomBookingCode ResultCodeSuccess_RoomAvability_ReturnsSuccessResultCode(bool roomAvailability)
        {
            if (!roomAvailability) {
            _availableStudyRoom.Clear(); // Mock room availability to false
            }
            return _bookingService.BookStudyRoom(_request).Code;
        }

        [TestCase(0, false)]
        [TestCase(55, true)]
        public void StudyRoomBooking_BookRoomWithAvailability_ReturnsBookingId
            (int expectedBookingId, bool roomAvailability)
        {

            if (!roomAvailability)
            {
                _availableStudyRoom.Clear();
              
            }


           // StudyRoomBooking savedStudyRoomBooking = null;
            _studyRoomBookingRepoMock.Setup(x => x.Book(It.IsAny<StudyRoomBooking>()))
                .Callback<StudyRoomBooking>(b =>
                { b.BookingId = 55; });

            var result = _bookingService.BookStudyRoom(_request);
            ClassicAssert.AreEqual(expectedBookingId, result.BookingId);
            if (!roomAvailability)
            {
                
                _studyRoomBookingRepoMock.Verify(x => x.Book(It.IsAny<StudyRoomBooking>()), Times.Never);
            }
        }

        [Test]
        public void BookNotInvoked_SaveBookingWithoutAvailableRoom_BookMethodNotInvoked()
        {
            _availableStudyRoom.Clear();

            var result = _bookingService.BookStudyRoom(_request);
      
            _studyRoomBookingRepoMock.Verify(x => x.Book(It.IsAny<StudyRoomBooking>()), Times.Never);            
        }
    }
}
