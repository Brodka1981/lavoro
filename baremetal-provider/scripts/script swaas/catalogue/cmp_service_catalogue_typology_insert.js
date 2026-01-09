db = db.getSiblingDB("cmp-service-catalogue");
db.runCommand({
    "delete": "typology",
    "deletes": [
      {
        "q": {_id:"swaas"},
        "limit": 0
      }
    ],
    "ordered": true
  });

  db.runCommand({
    "insert": "typology",
    "documents": [
      {
        "_id": "swaas",
        "title": "PROVISIONING_FILE.PROVISIONING.LAYOUT.CARD.TITLE.SWAAS",
        "description": "PROVISIONING_FILE.PROVISIONING.LAYOUT.CARD.DESCRIPTION.SWAAS",
        "category": "BaremetalNetwork",
        "icon": "cloud-load-balancer",
        "categoryId": "6716672bb0d047a0c51d0083",
        "name": "Swaases",
        "enableEcommerce": true,
        "showInCreationCard": false,
        "idReference": "KB_swaas",
        "projectRequired": true,
        "showInCategory": true,
        "enableDetailPage": true,
        "controllerName": "swaases",
        "filters": [
          "ResellerCustomerMenuFilter"
        ]
      }
    ],
    "ordered": false
  });