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

![Cummulative monthly CoinJoin volume](https://i.imgur.com/g3ydCzE.png)

### Analysis

JoinMarket style transactions have been rather stable over the last three years, with roughly 10000 bitcoin as a steady base, and reaching up to 25000 bitcoin in some months.

Wasabi started with only a fistful of bitcoin in the early months, but then rose sharply in one month to 18000 bitcoin. Following is a volatile but steady growth, reaching new all-time heights of nearly 45000 bitcoin recently.

Whirlpool transactions make a small percentage of overall volume, but nevertheless, enjoys steady growth in recent months.`

## Fresh bitcoin

### Explanation

Fresh bitcoin is calculated by the sum value of the inputs, those have not been outputs in a previous CoinJoin or tx0. This shows the value of "new" or previously unmixed bitcoin that are now entering the CoinJoin for the first time. This metric gives an intuition of user adoption.

![Cumulative monthly fresh bitcoin volume](https://i.imgur.com/r2YQ2Wm.png)

### Analysis



## Average remix count

### Explanation

Average remix count is a derived metric of the relation of 'total volume / fresh bitcoin'. It shows how often an average user remixes his bitcoin. The higher this ratio, the better the average user privacy. However, this metric does not consider the anonymity set that is achieved by a single transaction.

![Percentage average remix count](https://i.imgur.com/MISX41S.png)

### Analysis



## Never mixed

### Explanation

Never mixed coins are those who were intended to be mixed, but were not and thus did not gain any privacy. For Wasabi and Joinmarket, this is the sum value of change outputs that were spent in a non-CoinJoin transaction. Change that is remixed is not included. For whirlpool, this is the sum value of outputs of tx0 transactions that were spent in a non-CoinJoin or non-tx0 transaction. Tx0 outputs that are spent in a CoinJoin are not counted.

![Cumulative nevermixed bitcoin JoinMarket](https://i.imgur.com/iCNIsxW.png)
![Cumulative nevermixed bitcoin Wasabi](https://i.imgur.com/RtbCS6H.png)
![Cumulative nevermixed bitcoin Whirlpool](https://i.imgur.com/3vAEtmW.png)

CoinJoin inefficiency is a derived matric of the percentage of the sum of nevermixed coins to the sum of fresh bitcoin. The lower this ratio the better, because a higher percentage of bitcoin who were intended to be mixed actually received privacy.

![Percentage of nevermixed bitcoin](https://i.imgur.com/AXiyTP2.png)

### Analysis



## CoinJoin income

### Explanation

CoinJoin income is the quantity of bitcoin that the central coordinators operated by zkSNACKs [Wasabi] and Muletools [Whirlpool] earned per month. Joinmarket is excluded from this metric, as there are unknown numbers of makers earning the CoinJoin fee. Further, without sophisticated algorithms it is hard to calculate the income of Wasabi CoinJoins since it stopped using a fixed fee address, thus the calculation is discontinued at that point.

![Monthly CoinJoin income Wasabi and Whirlpool](https://i.imgur.com/H2Czk1M.png)

The metric of average fees paid is derived as the percentage of income and value of fresh bitcoin of that month.

![Percentage average CoinJoin fee per user of Wasabi and Whirlpool](https://i.imgur.com/kAZ1y8U.png)

### Analysis



## PostMix consolidation

### Explanation

The metric of average postmix input merging describes the number of inputs in the first non-CoinJoin transaction. This is excluding remixes in future CoinJoin transaction.

![Average number of inputs consolidated postmix](https://i.imgur.com/zhrY7Jv.png)

### Analysis



## Inputs smaller than the minimum

### Explanation

The percentage of inputs below the minimum denomination is explicitly relevant for Wasabi. It gives an intuition of how many users need to consolidate coins in order to reach the minimum of roughly 0.1 bitcoin.

![Percentage of inputs below minimum denomination in Wasabi](https://i.imgur.com/FCtCB2K.png)

### Analysis



## Conclusion



## WabiSabi fixes this


