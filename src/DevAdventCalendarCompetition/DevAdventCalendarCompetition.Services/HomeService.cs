﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using AutoMapper;
using DevAdventCalendarCompetition.Repository.Interfaces;
using DevAdventCalendarCompetition.Repository.Models;
using DevAdventCalendarCompetition.Services.Interfaces;
using DevAdventCalendarCompetition.Services.Models;

namespace DevAdventCalendarCompetition.Services
{
    public class HomeService : IHomeService
    {
        private readonly IHomeRepository _homeRepository;
        private readonly IMapper _mapper;

        public HomeService(
            IHomeRepository homeRepository,
            IMapper mapper)
        {
            this._homeRepository = homeRepository;
            this._mapper = mapper;
        }

        public TestDto GetCurrentTest()
        {
            var test = this._homeRepository.GetCurrentTest();
            if (test == null || (test.StartDate.HasValue && test.StartDate.Value.Date != DateTime.Today))
            {
                return null;
            }

            var testDto = this._mapper.Map<TestDto>(test);
            return testDto;
        }

        public TestAnswerDto GetTestAnswerByUserId(string userId, int testId)
        {
            var testAnswer = this._homeRepository.GetTestAnswerByUserId(userId, testId);
            var testAnswerDto = this._mapper.Map<TestAnswerDto>(testAnswer);
            return testAnswerDto;
        }

        public List<TestDto> GetCurrentTests()
        {
            var testList = this._homeRepository.GetAllTests();
            var allTestsDtoList = this._mapper.Map<List<TestDto>>(testList);
            return allTestsDtoList;
        }

        public List<TestWithAnswerListDto> GetTestsWithUserAnswers()
        {
            var testList = this._homeRepository.GetTestsWithUserAnswers();
            var testWithAnswersDtoList = this._mapper.Map<List<TestWithAnswerListDto>>(testList);
            return testWithAnswersDtoList;
        }

        public Dictionary<int, List<TestResultDto>> GetAllTestResults()
        {
            var testResultDictionary = new Dictionary<int, List<TestResultDto>>();
            var week1results = this._homeRepository.GetTestResultsForWeek(1);

            if (week1results != null && week1results.Count > 0)
            {
                testResultDictionary.Add(1, this.FillResultsWithAnswersStats(1, this._mapper.Map<List<TestResultDto>>(week1results)));

                var week2results = this._homeRepository.GetTestResultsForWeek(2);

                if (week2results != null && week2results.Count > 0)
                {
                    testResultDictionary.Add(2, this.FillResultsWithAnswersStats(2, this._mapper.Map<List<TestResultDto>>(week2results)));

                    var week3results = this._homeRepository.GetTestResultsForWeek(3);

                    if (week3results != null && week3results.Count > 0)
                    {
                        testResultDictionary.Add(3, this.FillResultsWithAnswersStats(3, this._mapper.Map<List<TestResultDto>>(week3results)));

                        var fullResults = this._homeRepository.GetTestResultsForWeek(4);

                        if (fullResults != null && fullResults.Count > 0)
                        {
                            testResultDictionary.Add(4, this.FillResultsWithAnswersStats(4, this._mapper.Map<List<TestResultDto>>(fullResults)));
                        }
                    }
                }
            }

            return testResultDictionary;
        }

        public string CheckTestStatus(int testNumber)
        {
            var test = this._homeRepository.GetTestByNumber(testNumber);

            return test == null ? TestStatus.NotStarted.ToString() : test.Status.ToString();
        }

        public int GetCorrectAnswersCountForUser(string userId)
        {
            return this._homeRepository.GetCorrectAnswersCountForUser(userId);
        }

        public int GetUserPosition(string userId)
        {
            return this._homeRepository.GetUserPosition(userId);
        }

        public string PrepareUserEmailForRODO(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return string.Empty;
            }

            var emailMaskRegex = "(^([\\w\\.\\-]{3})|(\\w{1,2})).*(@[\\w\\.\\-]).*(.)$";

            return Regex.Replace(email, emailMaskRegex,
                m => string.IsNullOrEmpty(m.Groups[3].Value)
                    ? m.Groups[2].Value + "..." + m.Groups[4].Value + "..." + m.Groups[5].Value
                    : m.Groups[3].Value + "..." + m.Groups[4].Value + "..." + m.Groups[5].Value);
        }

        private List<TestResultDto> FillResultsWithAnswersStats(int weekNumber, List<TestResultDto> results)
        {
            foreach (var result in results)
            {
                DateTimeOffset dateFrom;
                DateTimeOffset dateTo;

                switch (weekNumber)
                {
                    case 1:
                        dateFrom = new DateTimeOffset(DateTime.Now.Year, 12, 1, 20, 0, 0, TimeSpan.Zero);
                        dateTo = new DateTimeOffset(DateTime.Now.Year, 12, 8, 20, 0, 0, TimeSpan.Zero);
                        break;
                    case 2:
                        dateFrom = new DateTimeOffset(DateTime.Now.Year, 12, 8, 20, 0, 0, TimeSpan.Zero);
                        dateTo = new DateTimeOffset(DateTime.Now.Year, 12, 15, 20, 0, 0, TimeSpan.Zero);
                        break;
                    case 3:
                        dateFrom = new DateTimeOffset(DateTime.Now.Year, 12, 15, 20, 0, 0, TimeSpan.Zero);
                        dateTo = new DateTimeOffset(DateTime.Now.Year, 12, 22, 20, 0, 0, TimeSpan.Zero);
                        break;
                    default:
                        dateFrom = new DateTimeOffset(DateTime.Now.Year, 12, 1, 20, 0, 0, TimeSpan.Zero);
                        dateTo = new DateTimeOffset(DateTime.Now.Year, 12, 25, 20, 0, 0, TimeSpan.Zero);
                        break;
                }

                var correctAnswersCount = this._homeRepository.GetCorrectAnswersCountForUserAndDateRange(result.UserId, dateFrom, dateTo);
                var wrongAnswersCount =
                    this._homeRepository.GetWrongAnswersCountForUserAndDateRange(result.UserId, dateFrom, dateTo);

                result.CorrectAnswersCount = correctAnswersCount;
                result.WrongAnswersCount = wrongAnswersCount;
            }

            return results;
        }
    }
}