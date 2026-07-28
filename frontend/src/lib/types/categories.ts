export type CategoryTone = "orange" | "blue" | "purple" | "green" | "red" | string;

export type CategoryAppliesTo = "event" | "festival";

export type CreateCategoryPayload = {
  title: string;
  description: string;
  tags: string[];
  color?: string;
  emoji?: string;
  isHoliday?: boolean;
  type?: CategoryAppliesTo;
};

export type CategoryResponse = {
  id: number;
  name: string;
  description: string;
  tag?: string[] | null;
  color: string;
  icon: string;
  isHoliday?: boolean;
  type: CategoryAppliesTo;
};
