using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableTwitDB
{
    private List<Tweet> tweets;

    private List<Tweet> usedTweets;

    public void StartDB()
    {
        tweets = new List<Tweet>();
        usedTweets = new List<Tweet>();

        TwitDB twitDB = new TwitDB();
        twitDB.StartDB();
        // Generate 4 random twits
        List<Twit> twits = twitDB.GetAllTwits();

        foreach (Twit twit in twits)
        {
            Tweet newTweet = new Tweet(
                twit.profile_pic,
                twit.username,
                twit.handle,
                twit.content,
                twit.triggered_traits,
                twit.agenda_score
            );
            newTweet.SetShouldBeInteractable(true);
            tweets.Add(newTweet);
        }
    }

    private Tweet PopSelectableTweet()
    {
        Tweet poppedTweet = tweets[tweets.Count -1];
        tweets.Remove(poppedTweet);
        usedTweets.Add(poppedTweet);

        return poppedTweet;
    }

    public List<Tweet> GetNumberOfTimelineTweets (int number)
    {
        List<Tweet> list = new List<Tweet>();
        if (tweets.Count < number)
        {
            while (tweets.Count > 0)
            {
                Tweet tweet = PopSelectableTweet();
            }

            ReloadTweets();
        }

        for (int i = 0; i < number; i++)
        {
            list.Add(PopSelectableTweet());
        }

        return list;
    }

    public List<Tweet> GetAllTimelineTweets()
    {
        List<Tweet> list = new List<Tweet>();
        foreach (Tweet tweet in tweets)
        {
            list.Add(tweet);
        }

        foreach (Tweet tweet in usedTweets)
        {
            list.Add(tweet);
        }

        return list;
    }

    public void ReloadTweets()
    {
        tweets = usedTweets;
        tweets.Reverse();
        usedTweets = new List<Tweet>();
    }

    public void DebugLogAllTweets()
    {
        List<Tweet> list = GetAllTimelineTweets();

        foreach (Tweet tweet in list)
        {
            Debug.Log(tweet.GetUserName());
            Debug.Log(tweet.GetTraits());
        }
    }
}