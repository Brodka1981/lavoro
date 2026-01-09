db = db.getSiblingDB("cmp-provider-baremetal");

  db.runCommand({
    "insert": "swaasCatalog",
    "documents": [
      {        
        "_id": ObjectId('68653b7900df674b32a84401'),
        "code": "SW14_22D",
        "itemId": "6536",
        "networksCount" : 1,
        "data":[
        {
          "language":"it",
          "model": "Switch as a Service - Taglia 1 VS L2",
        }]
      },
      {        
        "_id": ObjectId('68653b8200df674b32a84402'),
        "code": "SW15_22D",
        "itemId": "6537",
        "networksCount" : 3,
        "data":[
        {
          "language":"it",
          "model": "Switch as a Service - Taglia 3 VS L2",
        }]
      },
      {        
        "_id": ObjectId('68653b8b00df674b32a84403'),
        "code": "SW16_22D",
        "itemId": "6538",
        "networksCount" : 5,
        "data":[
        {
          "language":"it",
          "model": "Switch as a Service - Taglia 5 VS L2",
        }]
      },
      {        
        "_id": ObjectId('68653bad00df674b32a84404'),
        "code": "SW17_22D",
        "itemId": "6539",
        "networksCount" : 10,
        "data":[
        {
          "language":"it",
          "model": "Switch as a Service - Taglia 10 VS L2",
        }]
      }
    ],
    "ordered": false
  });