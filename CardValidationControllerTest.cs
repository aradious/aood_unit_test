using DRestTest.Controllers;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DRestTest.Models;
using System.Threading.Tasks;
using Moq;
using DRestTest.Interfaces.Services;
using DRestTest;
using DRestTest.Entities;
using DRestTest.Interfaces.IRepositories;

namespace DRestUnitTest.Controllers.Test
{
    public class CardValidationControllerTest
    {
        private readonly CardValidationController _cardValidationController;
        private readonly string ValidVisas = "4397435601344857";
        private readonly string ValidMasterCards = "5290389773644435";

        private const string _expiryLeapYear = "022028";
        private const string _expiryPrimeYear = "022027";

        #region Setup Mock
        public CardValidationControllerTest()
        {

            var loggerController = Mock.Of<ILogger<CardValidationController>>();
            var loggerService = Mock.Of<ILogger<CreditCardService>>();

            IOptions<AppSettings> _appSettings = Mock.Of<IOptions<AppSettings>>();
            DBContext _dbContext = new DBContext(_appSettings);
            ICreditCardRepository _iCreditCardRepository;

            var mockRepo = new Mock<ICreditCardRepository>();

            _ = mockRepo.Setup(r => r.GetCreditCardBlacklist("4397436601344857", "Visa"))
                .ReturnsAsync(new CreditCardBlacklist
                {
                    Id = 1,
                    CardNumber = "4397436601344857",
                    CardType = "Visa",
                    IsActive = true
                });

            _iCreditCardRepository = mockRepo.Object;
            var _iCreditCardService = new CreditCardService(_iCreditCardRepository, loggerService);

            _cardValidationController = new CardValidationController(_iCreditCardService, loggerController);
        }
        #endregion

        [Fact]
        public async Task Test_Visa_Valid()
        {
            //Test with 439743560134488
            var actionResult = await _cardValidationController.Validate(
                 new CardValidateRequest
                 {
                     CardNumber = ValidVisas,
                     ExpiryDate = _expiryLeapYear
                 });

            var okResult = actionResult as OkObjectResult;

            // Assert
            Assert.Equal(200, okResult.StatusCode);
            ServerResponse resp = okResult.Value as ServerResponse;

            //expect is Visa
            Assert.True(CreditCardType.Visa.ToString() == ((string)resp.Result), resp.RespDesc);

        }

        [Fact]
        public async Task Test_MasterCard_Valid()
        {
            //Test with 5290389773644435 with prime year 022027
            var actionResult = await _cardValidationController.Validate(
                    new CardValidateRequest
                    {
                        CardNumber = ValidMasterCards,
                        ExpiryDate = _expiryPrimeYear
                    }
                );

            var okResult = actionResult as OkObjectResult;

            // Assert
            Assert.Equal(200, okResult.StatusCode);
            ServerResponse resp = okResult.Value as ServerResponse;

            //expect is MasterCard
            Assert.True(CreditCardType.MasterCard.ToString() == ((string)resp.Result), resp.RespDesc);
        }

    }
}