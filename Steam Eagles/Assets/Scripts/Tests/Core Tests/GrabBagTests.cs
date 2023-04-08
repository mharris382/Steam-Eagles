using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoreLib;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GrabBagTests
{
    public static (int, int)[] TestNumbers = new (int, int)[]
    {
        (1, 4),
        (2, 5),
        (3, 8),
        (6, 1)
    };
    
    
    public static (string, int)[] TestStrings = new (string, int)[]
    {
        ("Jim", 4),
        ("Dwight", 5),
        ("Michael", 8),
        ("Pam", 1)
    };



    [Test]
    public void TestGrabBagOnNumbers()
    {
        var grabBag = new GrabBag<int>();
        grabBag.Init(TestNumbers);
        Dictionary<int, int> counts = new Dictionary<int, int>();
        foreach (var number in grabBag.GetItems(_ => true))
        {
            if(counts.ContainsKey(number))
                counts[number]++;
            else
                counts.Add(number, 1);
        }

        foreach (var testNumber in TestNumbers)
        {
            Assert.AreEqual(testNumber.Item2, counts[testNumber.Item1]);
        }
    }
    int runningTotal = 0;
    
    [Test]
    public void TestGrabCancelsOnDecline()
    {
        runningTotal = 0;
        var grabBag = new GrabBag<int>();
        grabBag.Init(TestNumbers);
        int sum = TestNumbers.Sum(tuple => tuple.Item1 * tuple.Item2);
        int declineMax = sum / 2;
      
        Predicate<int> autoDecline = i =>
        {
            if (i + runningTotal > declineMax)
            {
                return false;
            }
            return true;
        };
        
        foreach (var number in grabBag.GetItems(autoDecline))
        {
            runningTotal += number;
        }
        Assert.LessOrEqual(runningTotal, declineMax);
    }

    
    [Test]
    public void TestGrabBagOnStrings()
    {
        var grabBag = new GrabBag<string>();
        grabBag.Init(TestStrings);
        Dictionary<string, int> counts = new Dictionary<string, int>();
        foreach (var str in grabBag.GetItems(_ => true))
        {
            if(counts.ContainsKey(str))
                counts[str]++;
            else
                counts.Add(str, 1);
        }

        foreach (var testString in TestStrings)
        {
            Assert.AreEqual(testString.Item2, counts[testString.Item1]);
        }
    }

    private int rejectCount = 0;
    int currentCount = 0;
    [Test]
    public void TestResetOnAccept()
    {
        rejectCount = 0;
        var grabBag = new GrabBag<int>();
        var testNumbers = new (int, int)[]
        {
            (1, 1),
            (2, 1),
            (3, 1),
            (4, 1)
        };
        grabBag.Init(testNumbers);
        //refuse to accept anything other than 2 as the first item
        //refuse to accept anything other than 1 as the second item
        //refuse to accept anything other than 3 as the third item
        //last item must be 4
        
        int[] expected = new int[] {2, 1, 3, 4};
        Predicate<int> predicate = i =>
        {
            var target = expected[currentCount];
            if (i == target)
            {
                return true;
            }

            return false;
        };
        int[] results = new int[4];
        foreach (var item in grabBag.GetItems(predicate))
        {
            results[currentCount] = item;
            currentCount++;
        }

        for (int i = 0; i < results.Length; i++)
        {
            Assert.AreEqual(expected[i], results[i]);
        }
    }
}
