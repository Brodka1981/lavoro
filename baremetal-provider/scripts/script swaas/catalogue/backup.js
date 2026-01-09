//
//db cmp-service-catalogue
//
db = db.getSiblingDB("cmp-service-catalogue");

collection = "typology";
db.getCollection(collection).aggregate([{ $out: `${collection}_bkp_20250626` }]);

