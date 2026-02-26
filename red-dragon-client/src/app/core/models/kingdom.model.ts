export interface Kingdom {
  id: number;
  name: string;
  race: string;
  isMagicRace: boolean;
  land: number;
  gold: number;
  food: number;
  stone: number;
  budulec: number;
  budulecStored: number;
  weapons: number;
  mana: number;
  population: number;
  popularity: number;
  wages: number;
  education: number;
  turnsAvailable: number;
  turnsPerDay: number;
  maxTurns: number;
  turnNumber: number;
  age: number;
  currentSpecialBuilding: string | null;
  specialBuildingProgress: number;
  specialBuildingCost: number;
  coalitionId: number | null;
  coalitionName: string | null;
  coalitionRole: string | null;
  eraId: number;
  eraName: string | null;
  isProtected: boolean;
  buildings: Building[];
  militaryUnits: MilitaryUnit[];
  professions: Profession[];
}

export interface KingdomSummary {
  id: number;
  name: string;
  race: string;
  land: number;
  population: number;
  coalitionTag: string | null;
}

export interface AssignWorkersDto {
  professionType: string;
  workerCount: number;
}

export interface BuyLandDto {
  amount: number;
}

export interface Building {
  id: number;
  buildingType: string;
  displayName: string;
  category: string;
  description: string | null;
  quantity: number;
  level: number;
  isUnderConstruction: boolean;
  constructionCompletesAt: string | null;
}

export interface BuildingDefinition {
  id: number;
  buildingType: string;
  category: string;
  displayName: string;
  description: string | null;
  costGold: number;
  costBudulec: number;
  costLand: number;
  row: number;
  col: number;
  baseCost: number;
  buildTime: number;
  requiredBuildingType: string | null;
  requiredTechnology: string | null;
  isSpecial: boolean;
  bonusTurnsPerDay: number;
  productionBonus: number;
  defenseBonus: number;
  populationCapacity: number;
  canBuild: boolean;
  cannotBuildReason: string | null;
}

export interface ConstructBuildingDto {
  buildingType: string;
  quantity: number;
}

export interface MilitaryUnit {
  id: number;
  unitType: string;
  displayName: string;
  description: string | null;
  quantity: number;
  inTraining: number;
  trainingCompletesAt: string | null;
  attackPower: number;
  defensePower: number;
  upkeep: number;
}

export interface UnitDefinition {
  id: number;
  unitType: string;
  displayName: string;
  description: string | null;
  costGold: number;
  costWeapons: number;
  costFood: number;
  attackPower: number;
  defensePower: number;
  upkeep: number;
  requiredBuilding: string;
  requiredTech: string | null;
  trainingTime: number;
  canRecruit: boolean;
  cannotRecruitReason: string | null;
}

export interface RecruitUnitsDto {
  unitType: string;
  quantity: number;
}

export interface Profession {
  professionType: string;
  displayName: string;
  workerCount: number;
  noviceCount: number;
  maxCapacity: number;
  productionPerTurn: number;
  novicePercent: number;
}

export interface TechDefinition {
  id: number;
  techType: string;
  category: string;
  displayName: string;
  description: string | null;
  costGold: number;
  researchTime: number;
  requiredTech: string | null;
  requiredBuilding: string | null;
  effectType: string | null;
  effectValue: number;
  canResearch: boolean;
  cannotResearchReason: string | null;
}

export interface Research {
  id: number;
  techType: string;
  displayName: string;
  category: string;
  description: string | null;
  isCompleted: boolean;
  isInProgress: boolean;
  completesAt: string | null;
}

export interface Coalition {
  id: number;
  name: string;
  tag: string | null;
  leaderKingdomId: number | null;
  leaderName: string | null;
  memberCount: number;
  maxMembers: number;
  psoProgress: number;
  members: KingdomSummary[];
}

export interface BattleReport {
  id: number;
  attackerName: string;
  defenderName: string;
  battleType: string;
  result: string;
  attackerLosses: string | null;
  defenderLosses: string | null;
  resourcesStolen: string | null;
  landCaptured: number;
  occurredAt: string;
}

export interface GameMessage {
  id: number;
  senderName: string;
  receiverName: string;
  subject: string | null;
  body: string | null;
  isRead: boolean;
  sentAt: string;
}

export interface ServiceResult {
  success: boolean;
  message: string | null;
}

export interface TurnResult {
  success: boolean;
  message: string | null;
  turnsRemaining: number;
  deltas: { [key: string]: number };
}

export interface ForumPost {
  id: number;
  forumType: string;
  coalitionId: number | null;
  authorKingdomId: number;
  authorName: string;
  authorCoalitionTag: string | null;
  subject: string;
  body: string;
  parentPostId: number | null;
  createdAt: string;
  replyCount: number;
  replies: ForumPost[];
}

export interface CreateForumPost {
  subject: string;
  body: string;
  parentPostId: number | null;
}

export interface AuthResponse {
  token: string;
  username: string;
  kingdomId: number;
  expiresAt: string;
}
