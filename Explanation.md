# Dumplings
### Analysing historical CoinJoin metrics

## Reason and scope for this research

A Bitcoin transaction consumes inputs, and generates outputs, as a means to transfer the ownership of bitcoin. The output address defines and commits to the requirement that needs to be fulfilled in order to spend the coin. The input provides a valid proof for the spending condition. The address is thus the pseudonymous identity of the user, and there is no inherent need to use the same identity for every transaction.

The art of Bitcoin privacy is to break the link between the pseudonymous identities of a single user. There are several heuristics that are utilized by surveillance actors in order to guess the common ownership of multiple coins. Among many techniques and best practices to protect user privacy, CoinJoin transactions have established themselves to be an important tool in the arsenal of users.

Since early discussion about the technique by Satoshi, and the proper introduction by Maxwell, there have been multiple CoinJoin implementations with more or less success. The goal of our research is to analyse the usage patterns and historical trends of the three most widely used systems to date, JoinMarket, Wasabi and Whirlpool. It is limited in scope to publicly verifiable blockchain data, and thus excludes any active network level analysis. In this article we explain our methodological approach, define each metric, and compare the implementations in historical analysis. This is done in an effort to find inefficiencies of current implementations, and improve on them with future protocols.

## How to fingerprint CoinJoin transactions
In order to be able to give a comprehensive overview of the Bitcoin privacy-enhancing scene we analyzed all CoinJoin transactions. However, first we needed to find them all!

CoinJoin transactions are typically easy to detect. They consist of multiple inputs and outputs. Usually the number of outputs are slightly larger than the number of inputs as input UTXOs produce also non-mixed change outputs as well. Most of the times a CoinJoin transaction consists of dozens of inputs and outputs. For instance, have a look at [this gigantic Wasabi CoinJoin](https://blockstream.info/nojs/tx/e4a789d16a24a6643dfee06e018ad27648b896daae6a3577ae0f4eddcc4d9174) with more than 100 input UTXOs. Good luck in deanonymizing that, dear chain analysts!

On the other hand, Samourai CoinJoin transactions have 5 inputs and outputs making them straightforward to detect.  


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
One can observe a notable spike in Wasabi CoinJoin fresh bitcoin amounts around early August and mid-September of 2019. Approximately 20,000 fresh bitcoins entered Wasabi CoinJoins from the PlusToken scammers in [an attempt to mix](https://medium.com/@ErgoBTC/tracking-the-plustoken-whale-attempted-bitcoin-laundering-and-its-impact-on-wasabi-wallet-787c0d240192) their stolen wealth.


## Average remix count

### Explanation

Average remix count is a derived metric of the relation of 'total volume / fresh bitcoin'. It shows how often an average user remixes his bitcoin. The higher this ratio, the better the average user privacy. However, this metric does not consider the anonymity set that is achieved by a single transaction.

![Percentage average remix count](https://i.imgur.com/MISX41S.png)

### Analysis
Remixes are a vital element of CoinJoin privacy-enhancing techniques. During the course of remixing users drastically increase their anonymity sets. Therefore, for instance, Samourai and Wasabi incentivize their users to remix their funds. Generally speaking, if a coin participates in **_k_** remixes, then its gained anonymity set is **_k_** times the average anonymity set of the used CoinJoin platform. Lately, we see that Samourai achieves around 15 anonymity set for a coin in average, while Wasabi achieves TBD anonymity set for an average coin.

Sadly, JoinMarket users do not really apply remixes to enhance transaction privacy.

## Never mixed

### Explanation

Never mixed coins are those who were intended to be mixed, but were not and thus did not gain any privacy. For Wasabi and Joinmarket, this is the sum value of change outputs that were spent in a non-CoinJoin transaction. Change that is remixed is not included. For whirlpool, this is the sum value of outputs of tx0 transactions that were spent in a non-CoinJoin or non-tx0 transaction. Tx0 outputs that are spent in a CoinJoin are not counted.

![Cumulative nevermixed bitcoin JoinMarket](https://i.imgur.com/iCNIsxW.png)
![Cumulative nevermixed bitcoin Wasabi](https://i.imgur.com/RtbCS6H.png)
![Cumulative nevermixed bitcoin Whirlpool](https://i.imgur.com/3vAEtmW.png)

CoinJoin inefficiency is a derived matric of the percentage of the sum of nevermixed coins to the sum of fresh bitcoin. The lower this ratio the better, because a higher percentage of bitcoin who were intended to be mixed actually received privacy.

![Percentage of nevermixed bitcoin](https://i.imgur.com/AXiyTP2.png)

### Analysis
For the sake of blockvspace efficiency, it is essential that a CoinJoin transaction has as the least possible amount of unmixed UTXOs. However, the rigidity of current fixed-denomination CoinJoin transaction structures somewhat impede this. Wasabi regularly leaves more than 10% of its UTXOs unmixed.


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
There is a tradeoff between usability of Wasabi and opening up potential Denial of Service (DoS) attack vectors. Namely, a small base denomination would enable more users to join a CoinJoin transaction. However, on the other hand, it would make a potential DoS attack cheaper to disrupt the CoinJoin service. In current Wasabi, UTXOs with value less than the base deanonymization can enter the CoinJoin transaction, although their combined values should be more than the base denomination value. If a user registers such inputs for mixing, then the coordinator is able to link those UTXOs that they belong to the same user. This is a privacy leak and clearly unwanted.


## Conclusion



## WabiSabi fixes this


