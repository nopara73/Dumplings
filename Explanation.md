# Dumplings
### Analysing historical CoinJoin metrics

## Reason and scope for this research

A Bitcoin transaction consumes inputs, and generates outputs, as a means to transfer the ownership of bitcoin. The output address defines and commits to the requirement that needs to be fulfilled in order to spend the coin. The input provides a valid proof for the spending condition. The address is thus the pseudonymous identity of the user, and there is no inherent need to use the same identity for every transaction.

The art of Bitcoin privacy is to break the link between the pseudonymous identities of a single user. There are several heuristics that are utilized by surveillance actors in order to guess the common ownership of multiple coins. Among many techniques and best practices to protect user privacy, CoinJoin transactions have established themselves to be an important tool in the arsenal of users.

Since early discussion about the technique by Satoshi, and the proper introduction by Maxwell, there have been multiple CoinJoin implementations with more or less success. The goal of our research is to analyse the usage patterns and historical trends of the three most widely used systems to date, JoinMarket, Wasabi and Whirlpool. It is limited in scope to publicly verifiable blockchain data, and thus excludes any active network level analysis. In this article we explain our methodological approach, define each metric, and compare the implementations in historical analysis. This is done in an effort to find inefficiencies of current implementations, and improve on them with future protocols.

## How to fingerprint CoinJoin transactions



## Monthly volumes

### Explanation

The monthly volume of CoinJoin transactions is the sum of bitcoin of all the outputs. This includes both the anonymity set equal value output, as well as the non-private change outputs. It is a metric that indicates how much bitcoin the users are willing to trust having in a hotwallet, but not all of these bitcoin will reach sufficient privacy.

![Cummulative monthly CoinJoin volume](https://i.imgur.com/HIGDlH0.png)

### Analysis

JoinMarket style transactions have been rather stable over the last three years, with roughly 10000 bitcoin as a steady base, and reaching up to 25000 bitcoin in some months.

Wasabi started with only a fistful of bitcoin in the early months, but then rose sharply in one month to 18000 bitcoin. Following is a volatile but steady growth, reaching new all-time heights of nearly 45000 bitcoin recently.

Whirlpool transactions make a small percentage of overall volume, but nevertheless, enjoys steady growth in recent months.`

## Fresh bitcoin

### Explanation

### Analysis



## Average remix count

### Explanation

### Analysis



## Never mixed

### Explanation

### Analysis



## CoinJoin income

### Explanation

### Analysis



## PostMix consolidation

### Explanation

### Analysis



## Inputs smaller than the minimum

### Explanation

### Analysis



## Conclusion



## WabiSabi fixes this


